using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MyLittleWidget.Contracts;
using MyLittleWidget.Models;
using MyLittleWidget.Utils;
using MyLittleWidget.ViewModels;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;

namespace MyLittleWidget.Views.Pages
{
  public sealed partial class DeskTopCapturePage : Page
  {
    private DeskTopCaptureViewModel viewModel = new();
    private static Type _draggedElementType;
    public DeskTopCapturePage()
    {
      this.InitializeComponent();
      viewModel.timer.Interval = TimeSpan.FromMilliseconds(30);
      viewModel.timer.Tick += async (s, e) => await RefreshCaptureAsync();
      this.Loaded += DeskTopCapturePage_Loaded;
    }

    // 将窗口相关的初始化代码移到 Loaded 事件处理程序中
    private void DeskTopCapturePage_Loaded(object sender, RoutedEventArgs e)
    {
      // 此时，App.window 肯定已经被赋值了
      var _window = ((App)Application.Current).window;
      _window.VisibilityChanged += Window_VisibilityChanged;


      if (viewModel.isdo == false)
      {
        var displayArea = DisplayArea.GetFromWindowId(_window.AppWindow.Id, DisplayAreaFallback.Primary);
        int targetWidth = _window.AppWindow.Size.Width;
        int targetHeight = (int)this.ActualHeight + (int)(550 * viewModel.Dpiscale);
        int centerX = displayArea.WorkArea.Width / 2 - targetWidth / 2 + displayArea.WorkArea.X;
        int centerY = displayArea.WorkArea.Height / 2 - targetHeight / 2 + displayArea.WorkArea.Y;
        RectInt32 rect = new RectInt32
        {
          X = centerX,
          Y = centerY,
          Width = targetWidth,
          Height = targetHeight
        };

        _window.AppWindow.MoveAndResize(rect, displayArea);
        viewModel.isdo = true;
      }
    }

    private async Task RefreshCaptureAsync()
    {
      using (var softwareBitmap = GetDesktop.CaptureWindow())
      {
        if (softwareBitmap != null)
        {
          viewModel.latestBitmap?.Dispose();
          viewModel.latestBitmap = CanvasBitmap.CreateFromSoftwareBitmap(CanvasDevice.GetSharedDevice(), softwareBitmap);
          DesktopCanvas.Invalidate();
        }
      }
    }

    private void AdjustWindowSizeToContent(Size desktopBitmapSize)
    {
      double maxPreviewHeight = desktopBitmapSize.Height / 7 * 4;
      double desktopAspectRatio = desktopBitmapSize.Width / desktopBitmapSize.Height;
      double previewHeight = maxPreviewHeight;
      double previewWidth = previewHeight * desktopAspectRatio;
      int targetWidth = (int)previewWidth;
      const double bottomControlsHeight = 200.0;
      int targetHeight = (int)previewHeight + (int)(bottomControlsHeight * viewModel.Dpiscale);

      var window = ((App)Application.Current).window;
      DisplayArea displayArea = DisplayArea.GetFromWindowId(window.AppWindow.Id, DisplayAreaFallback.Primary);

      int centerX = displayArea.WorkArea.Width / 2 - targetWidth / 2 + displayArea.WorkArea.X;
      int centerY = displayArea.WorkArea.Height / 2 - targetHeight / 2 + displayArea.WorkArea.Y;

      RectInt32 rect = new RectInt32
      {
        X = centerX,
        Y = centerY,
        Width = targetWidth,
        Height = targetHeight
      };

      window.AppWindow.MoveAndResize(rect, displayArea);
    }
    private void DesktopCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
      if (viewModel.latestBitmap != null)
      {
        if (viewModel.isdo == false)
        {
          LoadAndApplyWindowSize(viewModel.latestBitmap.Size);
          viewModel.isdo = true;
          sender.Invalidate();
          return;
        }
        var canvasSize = sender.Size;
        var imageSize = viewModel.latestBitmap.Size;

        double scale = Math.Min(canvasSize.Width / imageSize.Width, canvasSize.Height / imageSize.Height);

        double scaledWidth = imageSize.Width * scale;
        double scaledHeight = imageSize.Height * scale;

        double x = (canvasSize.Width - scaledWidth) / 2;
        double y = (canvasSize.Height - scaledHeight) / 2;

        Rect destinationRect = new Rect(x, y, scaledWidth, scaledHeight);

        args.DrawingSession.DrawImage(
            viewModel.latestBitmap,
            destinationRect,
            viewModel.latestBitmap.Bounds,
            1.0f,
            CanvasImageInterpolation.HighQualityCubic
        );
        DropInteractiveCanvas.Width = scaledWidth;
        DropInteractiveCanvas.Height = scaledHeight;
        
        DropInteractiveCanvas.Margin = new Thickness(x, y, 0, 0);

        SharedViewModel.Instance.Scale = scale * viewModel.Dpiscale;
      }
    }
    private void LoadAndApplyWindowSize(Size desktopBitmapSize)
    {
      var windowSize = Properties.Settings.Default.WindowSize;
      if (windowSize.Width > 200 &&
          windowSize.Height > 200)
      {
        var window = ((App)Application.Current).window;
        window.AppWindow.Resize(new SizeInt32((int)windowSize.Width, (int)windowSize.Height));
      }
      else
      {
        AdjustWindowSizeToContent(desktopBitmapSize);
      }
    }
    private void CaptureDesktopButton_Click(object sender, RoutedEventArgs e)
    {
      if (viewModel.timer.IsEnabled)
      {
        viewModel.timer.Stop();
        CaptureDesktopButton.Content = "开始预览";
        if (viewModel.latestBitmap != null)
        {
          viewModel.latestBitmap.Dispose();
          viewModel.latestBitmap = null;
        }
        DesktopCanvas.Draw -= DesktopCanvas_Draw;
        DesktopCanvas.Invalidate();
        CanvasDevice.GetSharedDevice().Trim();
        // 神奇,配合RefreshCaptureAsync的using可以让内存降低到比初始化的时候还低,但是缺一个都不行
        //GC.Collect();
        //GC.WaitForPendingFinalizers();
        //GC.Collect();

      }
      else
      {
        DesktopCanvas.Draw += DesktopCanvas_Draw;
        viewModel.timer.Start();
        CaptureDesktopButton.Content = "停止预览";
        //TODO 莫名其妙的有点时候重复订阅可以降低内存
        
      }

    }
    private void InteractiveCanvas_DragEnter(object sender, DragEventArgs e)
    {
      e.AcceptedOperation = DataPackageOperation.Move;
    }
    private void AddWidgetToCanvas(WidgetBase widget, Point positionOnPreview)
    {
      var scale = SharedViewModel.Instance.Scale;
      if (scale == 0) return;

      double desktopX = positionOnPreview.X / scale;
      double desktopY = positionOnPreview.Y / scale;

      widget.Config.PositionX = desktopX;
      widget.Config.PositionY = desktopY;

      SharedViewModel.Instance.WidgetList.Add(widget);
    }

    private void InteractiveCanvas_Drop(object sender, DragEventArgs e)
    {

      if (_draggedElementType != null)
      {
        try
        {

          object[] constructorArgs = new object[] { new WidgetConfig(), AppSettings.Instance };
          if (Activator.CreateInstance(_draggedElementType, constructorArgs) is WidgetBase newWidget)
          {
            if (sender is FrameworkElement canvas)
            {
              Point dropPosition = e.GetPosition(canvas);
              AddWidgetToCanvas(newWidget, dropPosition);
            }
          }
        }
        catch (Exception ex)
        {
          Debug.WriteLine($"Error during Drop: {ex.Message}");
        }
        finally
        {
          _draggedElementType = null;
        }
      }
    }

    private void GridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
    {
      if (e.Items.FirstOrDefault() is LittleWidget draggedWidget)
      {
        _draggedElementType = draggedWidget.widget.GetType();
      }
    }

    private void InteractiveCanvas_DragOver(object sender, DragEventArgs e)
    {
      e.AcceptedOperation = DataPackageOperation.Move;
    }
    private void Window_VisibilityChanged(object sender, WindowVisibilityChangedEventArgs args)
    {
      if (viewModel.timer.IsEnabled)
      {
        if (args.Visible)
        {
          viewModel.timer.Start();
        }
        else
        {
          viewModel.timer.Stop();
          if (viewModel.latestBitmap != null)
          {
            viewModel.latestBitmap.Dispose();
            viewModel.latestBitmap = null;
          }
          
        }
      }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      Environment.Exit(0);
    }
  }
}