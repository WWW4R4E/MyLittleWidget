using Microsoft.UI.Xaml.Input;
using MyLittleWidget.Contracts;
using MyLittleWidget.Services;
using MyLittleWidget.ViewModels;

namespace MyLittleWidget.Views
{
  public sealed partial class InteractiveCanvas : UserControl
  {
    private SharedViewModel _viewModel = SharedViewModel.Instance;
    private ConfigurationService _configService;
    private DeskTopCaptureViewModel _deskTopCaptureViewModel;
    private MenuFlyout _contextMenuWidget;  // 用于点击 Widget 时的菜单
    private MenuFlyout _contextMenuCanvas;  // 用于点击 Canvas 时的菜单
    private WidgetBase _rightClickedWidget;
    private Point _pointerOffset;

    public InteractiveCanvas()
    {
      _configService = new ConfigurationService();
      InitializeComponent();
    }
    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
      if (_rightClickedWidget != null)
      {
        _viewModel.WidgetList.Remove(_rightClickedWidget);
        _configService.Save();
      }
    }
    //private void SettingsMenuItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    //{
    //  // TODO:  实现设置逻辑
    //  Debug.WriteLine("Settings menu item clicked");
    //}
    // 在 InteractiveCanvas.cs 中

    private void PreviewCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
      var canvas = sender as Canvas;
      if (canvas == null) return;
      var currentPoint = e.GetCurrentPoint(canvas).Position;
      WidgetBase hitWidget = FindWidgetAtPosition(currentPoint);
      _rightClickedWidget = hitWidget;
      // 拖动widget逻辑
      if (hitWidget != null)
      {
        canvas.CapturePointer(e.Pointer);
        _viewModel.ActiveWidget = hitWidget;
        _viewModel.IsDragging = true;
        _pointerOffset = new Point(currentPoint.X - (hitWidget.Config.PositionX * _viewModel.Scale),
          currentPoint.Y - (hitWidget.Config.PositionY * _viewModel.Scale));

        //  显示 XAML 中定义的 SelectionBox
        if (SelectionBox != null)
        {
          SelectionBox.Width = hitWidget.ActualWidth * _viewModel.Scale;
          SelectionBox.Height = hitWidget.ActualHeight * _viewModel.Scale;
          Canvas.SetLeft(SelectionBox, hitWidget.Config.PositionX * _viewModel.Scale);
          Canvas.SetTop(SelectionBox, hitWidget.Config.PositionY * _viewModel.Scale);
          SelectionBox.Visibility = Visibility.Visible;
        }
      }
      else
      {
        _viewModel.IsDragging = false;
        if (SelectionBox != null)
        {
          SelectionBox.Visibility = Visibility.Collapsed;
        }
      }
    }
    private WidgetBase FindWidgetAtPosition(Point point)
    {
      for (int i = _viewModel.WidgetList.Count - 1; i >= 0; i--)
      {
        var widget = _viewModel.WidgetList[i];
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
        if (previewRect.Contains(point))
        {
          return widget;
        }
      }
      return null;
    }

    private void PreviewCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
      if (_viewModel.IsDragging)
      {
        var currentPoint = e.GetCurrentPoint(sender as Canvas).Position;

        double previewX = currentPoint.X - _pointerOffset.X;
        double previewY = currentPoint.Y - _pointerOffset.Y;

        if (SelectionBox != null && _viewModel.ActiveWidget != null)
        {
          Canvas.SetLeft(SelectionBox, previewX);
          Canvas.SetTop(SelectionBox, previewY);
        }
        _viewModel.UpdatePositionFromPreview(previewX, previewY);
      }
    }

    private void PreviewCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
      if (_viewModel.IsDragging)
      {
        _viewModel.ActiveWidget = null;
        _viewModel.IsDragging = false;
        var canvas = sender as Canvas;
        canvas.ReleasePointerCapture(e.Pointer);
        if (SelectionBox != null)
        {
          SelectionBox.Visibility = Visibility.Collapsed;
        }
        _configService.Save();
      }
    }
  }
}