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
    private MenuFlyout _contextMenuWidget;  // 用于点击 Widget 时的菜单
    private MenuFlyout _contextMenuCanvas;  // 用于点击 Canvas 时的菜单
    private WidgetBase _rightClickedWidget; 
    private Point _pointerOffset;

    public InteractiveCanvas()
    {
      _configService = new ConfigurationService();
      InitializeComponent();
      SetupContextMenu();
    }
    private void SetupContextMenu()
    {
      // 创建 Widget 菜单
      _contextMenuWidget = new MenuFlyout();
      var deleteMenuItem = new MenuFlyoutItem { Text = "删除" };
      deleteMenuItem.Click += DeleteMenuItem_Click;
      _contextMenuWidget.Items.Add(deleteMenuItem);
      _contextMenuWidget.Items.Add(new MenuFlyoutSeparator());

      // 创建 Canvas 菜单
      _contextMenuCanvas = new MenuFlyout();
      var settingsMenuItem = new MenuFlyoutItem { Text = "设置" };
      _contextMenuCanvas.Items.Add(settingsMenuItem); //设置菜单项
    }
    private void DeleteMenuItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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

#if DEBUG
            var existingDebugRect = canvas.Children.FirstOrDefault(c => c is Microsoft.UI.Xaml.Shapes.Rectangle && (c as Microsoft.UI.Xaml.Shapes.Rectangle).Name == "DebugRect");
            if (existingDebugRect != null)
            {
                canvas.Children.Remove(existingDebugRect);
            }
#endif

      if (e.GetCurrentPoint(canvas).Properties.IsRightButtonPressed)
      {
#if DEBUG
                if (hitWidget != null)
                {
                    var previewRect = new Rect(
                        hitWidget.Config.PositionX * _viewModel.Scale,
                        hitWidget.Config.PositionY * _viewModel.Scale,
                        hitWidget.ActualWidth * _viewModel.Scale,
                        hitWidget.ActualHeight * _viewModel.Scale
                    );
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
                }
#endif
        // 根据是否点击了 Widget 设置 ContextFlyout
        if (hitWidget != null)
        {
          InterCanvas.ContextFlyout = _contextMenuWidget;
          _contextMenuWidget.ShowAt(canvas, currentPoint);
        }
        else
        {
          InterCanvas.ContextFlyout = _contextMenuCanvas;
          _contextMenuCanvas.ShowAt(canvas, currentPoint);
        }

        return;
      }
      // 拖动widget逻辑
      if (hitWidget != null)
      {
#if DEBUG
                var previewRect = new Rect(
                    hitWidget.Config.PositionX * _viewModel.Scale,
                    hitWidget.Config.PositionY * _viewModel.Scale,
                    hitWidget.ActualWidth * _viewModel.Scale,
                    hitWidget.ActualHeight * _viewModel.Scale
                );
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
        canvas.CapturePointer(e.Pointer);
        _viewModel.ActiveWidget = hitWidget;
        _viewModel.IsDragging = true;
        _pointerOffset = new Point(currentPoint.X - (hitWidget.Config.PositionX * _viewModel.Scale),
                                 currentPoint.Y - (hitWidget.Config.PositionY * _viewModel.Scale));
      }
      else
      {
        _viewModel.IsDragging = false;
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
      if(_viewModel.IsDragging)
      {
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
}