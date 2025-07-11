using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MyLittleWidget.Utils;
using MyLittleWidget.ViewModels;
using Windows.ApplicationModel.DataTransfer;

namespace MyLittleWidget.Views.Pages
{
  public sealed partial class DeskTopCapturePage : Page
  {
    private DeskTopCaptureViewModel viewModel = new();

    public DeskTopCapturePage()
    {
      this.InitializeComponent();
      viewModel.timer.Interval = TimeSpan.FromMilliseconds(30);
      viewModel.timer.Tick += async (s, e) => await RefreshCaptureAsync();
      Console.WriteLine();
    }

    private void DeskTopCapturePage_Loaded(float canvasWidth, float canvasHeight)
    {
      if (viewModel.isdo == false)
      {
        var window = ((App)Application.Current).window;
        DisplayArea displayArea = DisplayArea.GetFromWindowId(window.AppWindow.Id, DisplayAreaFallback.Primary);
        int targetWidth = window.AppWindow.Size.Width;
        int targetHeight = (int)canvasHeight + (int)(550 * viewModel.Dpiscale);
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

    private void DesktopCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
      if (viewModel.latestBitmap != null)
      {
        var canvasSize = sender.Size;
        float canvasWidth = (float)canvasSize.Width;
        float canvasHeight = (float)canvasSize.Height;
        float imageWidth = viewModel.latestBitmap.SizeInPixels.Width;
        float imageHeight = viewModel.latestBitmap.SizeInPixels.Height;
        // 计算缩放比例，(逻辑像素/物理像素)
        viewModel.scale = Math.Min(canvasWidth / imageWidth, canvasHeight / imageHeight) * viewModel.Dpiscale;
        SharedViewModel.Instance.Scale = viewModel.scale;
        DeskTopCapturePage_Loaded(canvasWidth, canvasHeight);
        args.DrawingSession.DrawImage(
            viewModel.latestBitmap,
            new Rect(0, 0, canvasWidth, canvasHeight),
            new Rect(0, 0, imageWidth, imageHeight),
            1.0f,
            CanvasImageInterpolation.HighQualityCubic
        );
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
        DesktopCanvas.Invalidate();
        // 神奇,配合RefreshCaptureAsync的using可以让内存降低到比初始化的时候还低,但是缺一个都不行
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
      }
      else
      {
        viewModel.timer.Start();
        CaptureDesktopButton.Content = "停止预览";
      }
    }

    private void GridView_DragOver(object sender, DragEventArgs e)
    {
      e.AcceptedOperation = DataPackageOperation.Move;
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
      viewModel.timer.Stop();
      viewModel.latestBitmap?.Dispose();
      viewModel.latestBitmap = null;

      DesktopCanvas.RemoveFromVisualTree();
      DesktopCanvas = null;

      GC.Collect();
      GC.WaitForPendingFinalizers();
    }
  }
}