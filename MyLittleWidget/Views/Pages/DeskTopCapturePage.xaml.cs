using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MyLittleWidget.Contracts;
using MyLittleWidget.Models;
using MyLittleWidget.Utils;
using MyLittleWidget.ViewModels;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Input;

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
      Loaded += DeskTopCapturePage_Loaded;
    }

    private void DeskTopCapturePage_Loaded(object sender, RoutedEventArgs e)
    {
      viewModel.PreviewFrameReady += OnPreviewFrameReady;
      LoadAndApplyWindowSize();
    }

    private async Task RefreshCaptureAsync()
    {
      using (var softwareBitmap = GetDesktop.CaptureWindow())
      {
        if (softwareBitmap != null)
        {
          viewModel.LatestBitmap?.Dispose();
          viewModel.LatestBitmap = CanvasBitmap.CreateFromSoftwareBitmap(CanvasDevice.GetSharedDevice(), softwareBitmap);
          DesktopCanvas.Invalidate();
        }
      }
    }

    private void AdjustWindowSizeToContent()
    {
      RECT desktopBitmapSize = GetDesktop.GetDesktopGridInfo().rcWorkArea;
      double maxPreviewHeight = desktopBitmapSize.Height / 7 * 4;
      double desktopAspectRatio = desktopBitmapSize.Width / desktopBitmapSize.Height;
      double previewHeight = maxPreviewHeight;
      double previewWidth = previewHeight * desktopAspectRatio;
      int targetWidth = (int)previewWidth;
      const double bottomControlsHeight = 200.0;
      int targetHeight = (int)previewHeight + (int)(bottomControlsHeight * viewModel.Dpiscale);

      var window = ((App)Application.Current).MainWindow;
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
    private void OnPreviewFrameReady()
    {
      DesktopCanvas.Invalidate();
    }
    private void DesktopCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
      if (viewModel.LatestBitmap != null)
      {
        var canvasSize = sender.Size;
        var imageSize = viewModel.LatestBitmap.Size;
        double scale = Math.Min(canvasSize.Width / imageSize.Width, canvasSize.Height / imageSize.Height);
        double scaledWidth = imageSize.Width * scale;
        double scaledHeight = imageSize.Height * scale;
        double x = (canvasSize.Width - scaledWidth) / 2;
        double y = (canvasSize.Height - scaledHeight) / 2;
        args.DrawingSession.DrawImage(viewModel.LatestBitmap, new Rect(x, y, scaledWidth, scaledHeight));
        DropInteractiveCanvas.Width = scaledWidth;
        DropInteractiveCanvas.Height = scaledHeight;
        DropInteractiveCanvas.Margin = new Thickness(x, y, 0, 0);
        DropInteractiveCanvas.Margin = new Thickness(x, y, 0, 0);

        SharedViewModel.Instance.Scale = scale * viewModel.Dpiscale;
      }
      else
      {
        // 如果 LatestBitmap 为 null，清除画布
        args.DrawingSession.Clear(Windows.UI.Color.FromArgb(0, 0, 0, 0));
      }
    }

    private void LoadAndApplyWindowSize()
    {
      var windowSize = Properties.Settings.Default.WindowSize;
      if (windowSize.Width > 200 &&
          windowSize.Height > 200)
      {
        var window = ((App)Application.Current).MainWindow;
        window.AppWindow.Resize(new SizeInt32((int)windowSize.Width, (int)windowSize.Height));
      }
      else
      {
        AdjustWindowSizeToContent();
      }
    }
    private void CaptureDesktopButton_Click(object sender, RoutedEventArgs e)
    {
      if (viewModel.timer.IsEnabled)
      {
        viewModel.timer.Stop();
        CaptureDesktopButton.Content = "开始预览";
        if (viewModel.LatestBitmap != null)
        {
          viewModel.LatestBitmap.Dispose();
          viewModel.LatestBitmap = null;
        }
        DesktopCanvas.Draw -= DesktopCanvas_Draw;
        DesktopCanvas.Invalidate();
        CanvasDevice.GetSharedDevice().Trim();

      }
      else
      {
        DesktopCanvas.Draw += DesktopCanvas_Draw;
        viewModel.timer.Start();
        CaptureDesktopButton.Content = "停止预览";
        
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
    private void Button_Click(object sender, RoutedEventArgs e)
    {
      Application.Current.Exit();
    }

    private void InteractiveCanvas_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
      viewModel.IsPreviewing = true;
    }

    private void InteractiveCanvas_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
      viewModel.IsPreviewing = false;
    }
  }
}