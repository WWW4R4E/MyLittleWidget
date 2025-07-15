using CommunityToolkit.Mvvm.ComponentModel;
using MyLittleWidget.Contracts;

namespace MyLittleWidget.ViewModels;

public partial class SharedViewModel : ObservableObject
{
  private static readonly SharedViewModel _instance = new SharedViewModel();
  public static SharedViewModel Instance => _instance;

  [ObservableProperty]
  private double _scale = 1.0;

  [ObservableProperty]
  private bool _isDragging;

  [ObservableProperty]
  private WidgetBase _activeWidget;

  // 定义组件
  [ObservableProperty]
  private ObservableCollection<WidgetBase> _widgetBases;

  // 吸附的距离阈值
  private const double SnapThreshold = 10.0;

  // 辅助线
  private List<double> _verticalGuides = new List<double>();

  private List<double> _horizontalGuides = new List<double>();

  // 配置辅助线
  public void ConfigureGuides(List<double> vertical, List<double> horizontal)
  {
    _verticalGuides = vertical;
    _horizontalGuides = horizontal;
  }

  // 配置组件组
  public void ConfigureWidget(ObservableCollection<WidgetBase> widget)
  {
    _widgetBases = widget;
  }

  // 更新位置
  public void UpdateActiveWidgetPosition(double newX, double newY)
  {
    if (ActiveWidget == null) return;

    var snappedPosition = CheckAndApplySnapping(newX, newY, ActiveWidget.ActualWidth, ActiveWidget.ActualHeight);

    // 更新的是活动组件的位置属性
    ActiveWidget.Config.PositionX = snappedPosition.X;
    ActiveWidget.Config.PositionY = snappedPosition.Y;
    ActiveWidget.Config.PositionX = snappedPosition.X;
    ActiveWidget.Config.PositionY = snappedPosition.Y;
  }

  // 从PreviewWindow开始处理计算
  public void UpdatePositionFromPreview(double previewX, double previewY)
  {
    if (ActiveWidget == null) return;
    double actualX = previewX / Scale;
    double actualY = previewY / Scale;

    UpdateActiveWidgetPosition(actualX, actualY);
  }

  // 吸附计算
  private Point CheckAndApplySnapping(double newX, double newY, double width, double height)
  {
    double finalX = newX;
    double finalY = newY;

    // 垂直吸附
    var borderEdgesX = new[] { newX, newX + width / 2, newX + width };
    foreach (double guideX in _verticalGuides)
    {
      if (Math.Abs(borderEdgesX[0] - guideX) < SnapThreshold) { finalX = guideX; break; }
      if (Math.Abs(borderEdgesX[2] - guideX) < SnapThreshold) { finalX = guideX - width; break; }
      if (Math.Abs(borderEdgesX[1] - guideX) < SnapThreshold) { finalX = guideX - width / 2; break; }
    }

    // 水平吸附
    var borderEdgesY = new[] { newY, newY + height / 2, newY + height };
    foreach (double guideY in _horizontalGuides)
    {
      if (Math.Abs(borderEdgesY[0] - guideY) < SnapThreshold) { finalY = guideY; break; }
      if (Math.Abs(borderEdgesY[2] - guideY) < SnapThreshold) { finalY = guideY - height; break; }
      if (Math.Abs(borderEdgesY[1] - guideY) < SnapThreshold) { finalY = guideY - height / 2; break; }
    }

    return new Point(finalX, finalY);
  }
}