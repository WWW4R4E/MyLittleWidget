using MyLittleWidget.Contracts;

namespace MyLittleWidget.Services;

public class WidgetFactoryService
{
  private readonly IApplicationSettings _appSettings;
  private readonly IWidgetToolService _toolService;
  private readonly Dictionary<Type, object> _parameterCache;

  public WidgetFactoryService(IApplicationSettings appSettings, IWidgetToolService toolService)
  {
    _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    _toolService = toolService ?? throw new ArgumentNullException(nameof(toolService));

    // 缓存所有可能的参数类型
    _parameterCache = new Dictionary<Type, object>
            {
                { typeof(IApplicationSettings), _appSettings },
                { typeof(IWidgetToolService), _toolService },
            };
  }

  /// <summary>
  /// 根据 Widget 类型和配置创建 Widget 实例。
  /// </summary>
  /// <param name="config">Widget 配置</param>
  /// <param name="widgetType">Widget 类型（可选，优先于 config.WidgetType）</param>
  /// <returns>创建的 WidgetBase 实例或 null</returns>
  public WidgetBase? CreateWidgetFromType(WidgetConfig config, Type? widgetType = null)
  {
    if (config == null) throw new ArgumentNullException(nameof(config));

    // 优化类型查找，支持多个程序集
    Type? targetType = widgetType;
    if (targetType == null && !string.IsNullOrEmpty(config.WidgetType))
    {
      // 先尝试直接用 Type.GetType（带程序集名的情况）
      targetType = Type.GetType(config.WidgetType);
      if (targetType == null)
      {
        // 遍历所有已加载程序集查找类型 TODO 优化为加载约定部分的程序集
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in allAssemblies)
        {
          targetType = asm.GetType(config.WidgetType);
          if (targetType != null)
            break;
        }
      }
    }

    if (targetType == null || !typeof(WidgetBase).IsAssignableFrom(targetType))
    {
      return null; // 无效类型
    }

    // 获取所有构造函数，按参数数量降序排序（优先匹配参数最多的构造函数）
    var constructors = targetType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
        .OrderByDescending(c => c.GetParameters().Length);

    foreach (var constructor in constructors)
    {
      var parameters = constructor.GetParameters();
      var args = new object?[parameters.Length];
      bool canConstruct = true;

      for (int i = 0; i < parameters.Length; i++)
      {
        var paramType = parameters[i].ParameterType;

        if (paramType == typeof(WidgetConfig))
        {
          args[i] = config;
        }
        else if (_parameterCache.TryGetValue(paramType, out var cachedParam))
        {
          args[i] = cachedParam;
        }
        else
        {
          if (parameters[i].HasDefaultValue)
          {
            args[i] = parameters[i].DefaultValue;
          }
          else
          {
            canConstruct = false;
            break;
          }
        }
      }

      if (canConstruct)
      {
        try
        {
          return constructor.Invoke(args) as WidgetBase;
        }
        catch (Exception)
        {
          // 继续尝试下一个构造函数
        }
      }
    }

    return null; // 没有匹配的构造函数
  }
}