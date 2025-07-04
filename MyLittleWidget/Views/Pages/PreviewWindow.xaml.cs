using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MyLittleWidget.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyLittleWidget.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PreviewWindow : Page
    {

        private SharedViewModel _viewModel = SharedViewModel.ViewModel;
        private bool _isDragging = false;
        private Point _pointerOffset;

        public PreviewWindow()
        {
            InitializeComponent();
            double mainWinWidth = 800; // 主窗口的实际宽度
            double previewCanvasWidth = 400; // 预览Canvas的宽度
            _viewModel.Scale = previewCanvasWidth / mainWinWidth;
        }
        private void PreviewCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = true;
            var canvas = sender as Canvas;
            canvas.CapturePointer(e.Pointer);

            var currentPoint = e.GetCurrentPoint(canvas).Position;

            // 计算鼠标在预览图中的偏移
            // 注意：这里用的是ViewModel的位置 * 缩放比例
            double borderLeftInPreview = _viewModel.PositionX * _viewModel.Scale;
            double borderTopInPreview = _viewModel.PositionY * _viewModel.Scale;

            _pointerOffset = new Point(currentPoint.X - borderLeftInPreview, currentPoint.Y - borderTopInPreview);
            _viewModel.IsDragging = true;
        }

        private void PreviewCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isDragging)
            {
                var currentPoint = e.GetCurrentPoint(sender as UIElement).Position;

                // 计算在预览图中的理论新位置
                double previewX = currentPoint.X - _pointerOffset.X;
                double previewY = currentPoint.Y - _pointerOffset.Y;

                // 调用ViewModel来更新位置，ViewModel内部会处理缩放转换
                _viewModel.UpdatePositionFromPreview(previewX, previewY, PreviewBorder.ActualWidth / _viewModel.Scale, PreviewBorder.ActualHeight / _viewModel.Scale);
            }
        }

        private void PreviewCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            var canvas = sender as Canvas;
            canvas.ReleasePointerCapture(e.Pointer);
            _viewModel.IsDragging = false;
        }
    }
}
