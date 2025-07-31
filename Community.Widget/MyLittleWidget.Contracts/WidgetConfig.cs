using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Text.Json;
using Windows.ApplicationModel.VoiceCommands;

namespace MyLittleWidget.Contracts
{
  public partial class WidgetConfig : ObservableObject
  {
    /// <summary>
    /// 小部件唯一标识。
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// 小部件名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 开发者名称
    /// </summary>
    public string Developer { get; set; }
    /// <summary>
    /// 当前小部件的宽度（单位）。
    /// </summary>
    public int UnitWidth { get; set; } = 2;

    /// <summary>
    /// 当前小部件的高度（单位）。
    /// </summary>
    public int UnitHeight { get; set; } = 2;

    /// <summary>
    /// 支持的单位尺寸规格，最多4种。
    /// </summary>
    public List<(int Width, int Height)> SupportedUnitSizes { get; set; } = new()
      {
        (2, 2)
      };
    /// <summary>
    /// 保存原始 JSON 数据，延迟解析。
    /// </summary>
    [ObservableProperty]
    private JsonElement? _customSettings;


    // 存储单个执行方法
    private System.Action _executeMethod;

    /// <summary>
    /// 设置小部件的执行方法
    /// </summary>
    /// <param name="method">要执行的方法</param>
    public void SetExecuteMethod(System.Action method)
    {
      _executeMethod = method;
    }

    /// <summary>
    /// 执行存储的方法
    /// </summary>
    public void ExecuteMethod()
    {
      _executeMethod?.Invoke();
    }

    /// <summary>
    /// 指示是否设置了执行方法
    /// </summary>
    public bool HasExecuteMethod => _executeMethod != null;

    [ObservableProperty]
    private double _positionX;

    [ObservableProperty]
    private double _positionY;

    [ObservableProperty]
    private string _widgetType;
  }
}