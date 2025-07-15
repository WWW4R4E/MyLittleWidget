using Microsoft.UI.Xaml.Input;
using MyLittleWidget.Contracts;
using MyLittleWidget.Services;
using MyLittleWidget.ViewModels;

namespace MyLittleWidget.Views
{
  public sealed partial class InteractiveCanvas : UserControl
  {
    private SharedViewModel _viewModel = SharedViewModel.Instance;
    private readonly ConfigurationService _configService;
    private bool _isDragging = false;
    private Point _pointerOffset;

    public InteractiveCanvas()
    {
      _configService = new ConfigurationService();
      InitializeComponent();
    }

    private void PreviewCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
      var canvas = sender as Canvas;
      if (canvas == null) return;

      var transform = canvas.TransformToVisual(null);

      Point canvasOriginInWindow = transform.TransformPoint(new Point(0, 0));

      var currentPoint = e.GetCurrentPoint(canvas).Position;

      WidgetBase hitWidget = null;
      for (int i = _viewModel.WidgetBases.Count - 1; i >= 0; i--)
      {
        var widget = _viewModel.WidgetBases[i];

        if (widget.ActualWidth == 0 || widget.ActualHeight == 0)
        {
          continue;
        }

        var finalScale = _viewModel.Scale;
        var previewRect = new Rect(
            widget.Config.PositionX * finalScale,
            widget.Config.PositionY * finalScale,
            widget.ActualWidth * finalScale,
            widget.ActualHeight * finalScale
        );
#if DEBUG
        var debugRect = new Microsoft.UI.Xaml.Shapes.Rectangle
        {
          Name = "DebugRect",
          Stroke = new SolidColorBrush(Colors.Red),
          StrokeThickness = 2
        };
        canvas.Children.Add(debugRect);

        debugRect.Width = previewRect.Width;
        debugRect.Height = previewRect.Height;
        Canvas.SetLeft(debugRect, previewRect.X);
        Canvas.SetTop(debugRect, previewRect.Y);
#endif

        if (previewRect.Contains(currentPoint))
        {
          hitWidget = widget;
          break;
        }
      }

      if (hitWidget != null)
      {
        _isDragging = true;
        canvas.CapturePointer(e.Pointer);
        _viewModel.ActiveWidget = hitWidget;
        _viewModel.IsDragging = true;

        // 计算偏移
        _pointerOffset = new Point(currentPoint.X - (hitWidget.Config.PositionX * _viewModel.Scale),
                                 currentPoint.Y - (hitWidget.Config.PositionY * _viewModel.Scale));
        _viewModel.IsDragging = true;
      }
    }

    private void PreviewCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
      if (_isDragging)
      {
        var currentPoint = e.GetCurrentPoint(sender as Canvas).Position;

        double previewX = currentPoint.X - _pointerOffset.X;
        double previewY = currentPoint.Y - _pointerOffset.Y;
#if DEBUG
        //  更新辅助框
        Canvas.SetLeft(SelectionBox, previewX);
        Canvas.SetTop(SelectionBox, previewY);
#endif
        _viewModel.UpdatePositionFromPreview(previewX, previewY);
      }
    }

    private void PreviewCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
      _isDragging = false;
      _viewModel.ActiveWidget = null;
      _viewModel.IsDragging = false;
      var canvas = sender as Canvas;
      canvas.ReleasePointerCapture(e.Pointer);
#if DEBUG
      canvas.Children.Clear();
#endif
      _configService.Save();
    }
  }
}