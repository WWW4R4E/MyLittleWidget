using CommunityToolkit.Mvvm.ComponentModel;

namespace MyLittleWidget.ViewModels;
public partial class SharedViewModel : ObservableObject
{
    private static readonly SharedViewModel _viewModel = new SharedViewModel();
    public static SharedViewModel ViewModel => _viewModel;
    [ObservableProperty]
    private double _positionX;

    [ObservableProperty]
    private double _positionY;

    [ObservableProperty]
    private double _scale = 1.0;
    [ObservableProperty]
    private bool _isDragging;
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

    // 更新位置
    public void UpdatePosition(double newX, double newY, double width, double height)
    {
        var snappedPosition = CheckAndApplySnapping(newX, newY, width, height);
        PositionX = snappedPosition.X;
        PositionY = snappedPosition.Y;
    }

    // 从PreviewWindow开始处理计算
    public void UpdatePositionFromPreview(double previewX, double previewY, double width, double height)
    {
        double actualX = previewX / Scale;
        double actualY = previewY / Scale;

        UpdatePosition(actualX, actualY, width, height);
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
