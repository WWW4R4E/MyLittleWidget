using Microsoft.UI.Xaml.Input;
using MyLittleWidget.Custom;
using MyLittleWidget.ViewModels;


namespace MyLittleWidget.Views
{
    public sealed partial class InteractiveCanvas : UserControl
    {

        private SharedViewModel _viewModel = SharedViewModel.Instance;
        private bool _isDragging = false;
        private Point _pointerOffset;

        public InteractiveCanvas()
        {
            InitializeComponent();
            double mainWinWidth = 400; 
            double previewCanvasWidth = 800; 
            _viewModel.Scale = previewCanvasWidth / mainWinWidth;
        }
        private void PreviewCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = true;
            var canvas = sender as Canvas;
            canvas.CapturePointer(e.Pointer);

            var currentPoint = e.GetCurrentPoint(canvas).Position;


            // 命中测试
            WidgetBase hitWidget = null;
            for (int i = _viewModel.WidgetBases.Count - 1; i >= 0; i--)
            {
                var widget = _viewModel.WidgetBases[i];
                var previewRect = new Rect(
                    widget.PositionX * _viewModel.Scale,
                    widget.PositionY * _viewModel.Scale,
                    widget.ActualWidth * _viewModel.Scale,
                    widget.ActualHeight * _viewModel.Scale
                );
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
                _pointerOffset = new Point(currentPoint.X - (hitWidget.PositionX * _viewModel.Scale),
                                         currentPoint.Y - (hitWidget.PositionY * _viewModel.Scale));
                _viewModel.IsDragging = true;
            }

        }

        private void PreviewCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isDragging)
            {
                var currentPoint = e.GetCurrentPoint(sender as UIElement).Position;

                double previewX = currentPoint.X - _pointerOffset.X;
                double previewY = currentPoint.Y - _pointerOffset.Y;

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
         
        }
    }
}
