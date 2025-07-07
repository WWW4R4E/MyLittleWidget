using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MyLittleWidget.CustomBase;
using MyLittleWidget.Models;
using MyLittleWidget.Utils;
using MyLittleWidget.ViewModels;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;

namespace MyLittleWidget.Views.Pages
{
    public sealed partial class DeskTopCapturePage : Page
    {
        DeskTopCaptureViewModel viewModel = new();
        private static Type _draggedElementType;
        public DeskTopCapturePage()
        {
            this.InitializeComponent();
            viewModel.timer.Interval = TimeSpan.FromMilliseconds(60);
            viewModel.timer.Tick += async (s, e) => await RefreshCaptureAsync();
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

                SharedViewModel.Instance.Scale = scale *viewModel.Dpiscale;
            }
        }
        private void LoadAndApplyWindowSize(Size desktopBitmapSize)
        {
            var windowSize = Properties.Settings.Default.WindowSize;
            // 尝试加载保存的尺寸
            if (windowSize.Width >200 &&
                windowSize.Height > 200)
            {
                // 如果成功加载，直接应用
                var window = ((App)Application.Current).window;
                window.AppWindow.Resize(new SizeInt32((int)windowSize.Width, (int)windowSize.Height));
            }
            else
            {
                // 如果没有保存的尺寸（第一次启动），则调用你的默认尺寸计算方法
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

            // 调用 Initialize 方法 (如果需要)
            widget.Initialize();

            SharedViewModel.Instance.WidgetList.Add(widget);
        }

        // 修改你的 Drop 事件处理
        private void InteractiveCanvas_Drop(object sender, DragEventArgs e)
        {
            if (_draggedElementType != null)
            {
                try
                {
                    // 使用 Activator.CreateInstance 创建一个新实例
                    // 这要求你的控件有一个无参数的构造函数
                    if (Activator.CreateInstance(_draggedElementType) is WidgetBase newWidget)
                    {
                        if (sender is FrameworkElement canvas)
                        {
                            Point dropPosition = e.GetPosition(canvas);
                            // 调用 AddWidgetToCanvas，传递这个“全新的”控件实例
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
                    // 清空类型信息
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

        // DragOver 应该只用来表明可以放置
        private void InteractiveCanvas_DragOver(object sender, DragEventArgs e)
        {
            // 告诉系统，这里可以接受一个“移动”操作
            e.AcceptedOperation = DataPackageOperation.Move;
        }
    }
}
