using MyLittleWidget.Contracts;

namespace MyLittleWidget.Services
{
  internal class ComponentRegistryService
  {
    private static readonly Dictionary<string, Type> _registeredWidgets = new Dictionary<string, Type>();

    public static bool IsInitialized { get; private set; } = false;

    // 应用启动时调用此方法
    public static void DiscoverWidgets()
    {
      if (IsInitialized) return;

      // 1. 扫描主程序集 (查找官方组件，如 CustomControl1, CustomControl2)
      var mainAssembly = Assembly.GetExecutingAssembly();
      DiscoverWidgetsInAssembly(mainAssembly);

      // 2. 扫描插件目录 (查找第三方组件)
      string baseDirectory = AppContext.BaseDirectory;
      string widgetsDirectory = Path.Combine(baseDirectory, "Widgets"); // 约定好的插件目录

      if (Directory.Exists(widgetsDirectory))
      {
        foreach (var dllFile in Directory.GetFiles(widgetsDirectory, "*.dll"))
        {
          try
          {
            var pluginAssembly = Assembly.LoadFrom(dllFile);
            DiscoverWidgetsInAssembly(pluginAssembly);
          }
          catch (Exception ex)
          {
            // 记录加载某个DLL失败的日志
            System.Diagnostics.Debug.WriteLine($"Failed to load plugin assembly {dllFile}: {ex.Message}");
          }
        }
      }

      IsInitialized = true;
      System.Diagnostics.Debug.WriteLine($"Widget discovery complete. Found {_registeredWidgets.Count} widgets.");
    }

    // 辅助方法：在单个程序集中查找组件
    private static void DiscoverWidgetsInAssembly(Assembly assembly)
    {
      try
      {
        var widgetTypes = assembly.GetTypes()
            .Where(t =>
                // 类型必须是 WidgetBase 的子类
                typeof(WidgetBase).IsAssignableFrom(t) &&
                // 类型不能是抽象类
                !t.IsAbstract &&
                // 类型必须是公共的
                t.IsPublic
            );

        foreach (var type in widgetTypes)
        {
          // 使用类型的完整名称作为键
          string typeName = type.FullName;
          if (!string.IsNullOrEmpty(typeName) && !_registeredWidgets.ContainsKey(typeName))
          {
            _registeredWidgets.Add(typeName, type);
            System.Diagnostics.Debug.WriteLine($"Registered widget: {typeName}");
          }
        }
      }
      catch (ReflectionTypeLoadException ex)
      {
        System.Diagnostics.Debug.WriteLine($"Could not inspect assembly {assembly.FullName}: {ex.Message}");
        // 可以进一步记录 ex.LoaderExceptions 来获取更详细的失败信息
      }
    }

    // 提供一个公共方法来获取已注册的类型
    public static Type GetWidgetType(string fullTypeName)
    {
      _registeredWidgets.TryGetValue(fullTypeName, out var type);
      return type;
    }

    // (可选) 提供一个获取所有组件类型的方法，用于在UI上显示可用组件列表
    public static IEnumerable<Type> GetAllWidgetTypes()
    {
      return _registeredWidgets.Values;
    }
}
}
