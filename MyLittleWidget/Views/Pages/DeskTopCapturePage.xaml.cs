using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using MyLittleWidget.Models;
using MyLittleWidget.Utils;
using MyLittleWidget.ViewModels;
using MyLittleWidget.Views;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;

namespace MyLittleWidget.Views.Pages
{
    public sealed partial class DeskTopCapturePage : Page
    {
        DeskTopCaptureViewModel viewModel = new();

        public DeskTopCapturePage()
        {
            this.InitializeComponent();
            viewModel.timer.Interval = TimeSpan.FromMilliseconds(33);
            viewModel.timer.Tick += async (s, e) => await RefreshCaptureAsync();
            Console.WriteLine();
            //_ = LoadWallpaperAsync();
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
        //private async Task LoadWallpaperAsync()
        //{
        //    var wallpaperPath = GetDesktop.GetWallpaperPath();

        //    if (File.Exists(wallpaperPath))
        //    {
        //        var file = await StorageFile.GetFileFromPathAsync(wallpaperPath);
        //        using var stream = await file.OpenAsync(FileAccessMode.Read);

        //        var bitmap = new BitmapImage();
        //        await bitmap.SetSourceAsync(stream);
        //        DesktopBackground.Source = bitmap;
        //    }
        //}


        private async Task RefreshCaptureAsync()
        {
            var softwareBitmap =  GetDesktop.CaptureWindow();
            if (softwareBitmap != null)
            {
                viewModel.latestBitmap?.Dispose();
                viewModel.latestBitmap = CanvasBitmap.CreateFromSoftwareBitmap(CanvasDevice.GetSharedDevice(), softwareBitmap);
                DesktopCanvas.Invalidate();
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
                // 计算缩放比例，按比例完整展示 (逻辑像素/bitmap_Width (物理像素)
                viewModel.scale = Math.Min(canvasWidth / imageWidth, canvasHeight / imageHeight) *viewModel.Dpiscale;
                SharedViewModel.Instance.Scale = viewModel.scale;
                DeskTopCapturePage_Loaded(canvasWidth, canvasHeight);
                args.DrawingSession.DrawImage(
                    viewModel.latestBitmap,
                    new Rect(0, 0, canvasWidth , canvasHeight),
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
            }
            else
            {
                viewModel.timer.Start();
                CaptureDesktopButton.Content = "停止预览";
            }

        }
        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    string paths = "";
                    foreach (var file in items)
                    {
                        paths += file.Path + "\n";
                    }
                    Debug.WriteLine(paths.Trim());
                }
            }
        }
        private void GridView_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;

            //var items = e.DataView.GetStorageItemsAsync().GetResults();
            //Debug.WriteLine(items.ToString());
        }

        private void Border_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }

        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            if (sender is Border border)
            {
                // 获取绑定到该 Border 的 GridItem 数据
                var gridItem = border.DataContext as GridItem;

                if (gridItem != null)
                {
                    // 现在你可以操作 gridItem 了
                    System.Diagnostics.Debug.WriteLine($"Clicked on GridItem: Row={gridItem._position.X}, Column={gridItem._position.Y}");
                }
            }
        }

        private void StopCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            //var size = GetDesktop.GetDesktopGridInfo();
            //var rect = size.rcWorkArea;
            //var childWindow = ((App)App.Current).childWindow = new GridContainerWindow(rect);
            //childWindow.Activate();
        }
    }
}
