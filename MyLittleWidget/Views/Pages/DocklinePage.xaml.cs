using Microsoft.UI.Xaml.Input;
using MyLittleWidget.ViewModels;

namespace MyLittleWidget.Views.Pages
{

    public sealed partial class DocklinePage : Page
    {
        public SharedViewModel ViewModel { get; } = SharedViewModel.ViewModel;
        //private bool isDragging = false;
        // 用于存储拖动开始时，指针相对于 Border 左上角的偏移
        private Point _pointerOffset;

        // --- 吸附逻辑相关 ---
        // 吸附的距离阈值，当边缘距离辅助线小于这个值时，触发吸附
        private const double SnapThreshold = 10.0;
        // 定义垂直和水平辅助线的位置
        private readonly List<double> _verticalGuides = new List<double> { 250, 500 };
        private readonly List<double> _horizontalGuides = new List<double> { 250, 400 };
        public DocklinePage()
        {
            InitializeComponent();
            SetupGuideLines();
            ViewModel.ConfigureGuides(_verticalGuides, _horizontalGuides);

            ViewModel.PositionX = 100;
            ViewModel.PositionY = 100;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged_ForGuideVisibility;
            this.Unloaded += (s, e) => {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged_ForGuideVisibility;
            };

            SetupGuideLines(); // 这个方法现在只负责创建和布局Line元素
        }

        // 这个事件处理器现在只关心“显示”辅助线，不再计算任何东西
        private void ViewModel_PropertyChanged_ForGuideVisibility(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // 我们现在关心坐标和拖动状态的变化
            if (e.PropertyName == nameof(ViewModel.PositionX) ||
                e.PropertyName == nameof(ViewModel.PositionY) ||
                e.PropertyName == nameof(ViewModel.IsDragging))
            {
                // ★★★★★ 核心修改 5: 只有在拖动时才更新辅助线 ★★★★★
                if (ViewModel.IsDragging)
                {
                    UpdateGuideVisibility();
                }
                else
                {
                    // 如果拖动结束，就隐藏所有辅助线
                    HideAllGuides();
                }
            }
        }
        private void SetupGuideLines()
        {
            // 设置垂直辅助线的位置和长度
            Canvas.SetLeft(VerticalGuide1, _verticalGuides[0]);
            VerticalGuide1.Y1 = 0;
            VerticalGuide1.Y2 = RootCanvas.ActualHeight;

            Canvas.SetLeft(VerticalGuide2, _verticalGuides[1]);
            VerticalGuide2.Y1 = 0;
            VerticalGuide2.Y2 = RootCanvas.ActualHeight;

            // 设置水平辅助线的位置和长度
            Canvas.SetTop(HorizontalGuide1, _horizontalGuides[0]);
            HorizontalGuide1.X1 = 0;
            HorizontalGuide1.X2 = RootCanvas.ActualWidth;

            Canvas.SetTop(HorizontalGuide2, _horizontalGuides[1]);
            HorizontalGuide2.X1 = 0;
            HorizontalGuide2.X2 = RootCanvas.ActualWidth;

            // 当窗口大小变化时更新辅助线长度
            RootCanvas.SizeChanged += (s, e) =>
            {
                VerticalGuide1.Y2 = e.NewSize.Height;
                VerticalGuide2.Y2 = e.NewSize.Height;
                HorizontalGuide1.X2 = e.NewSize.Width;
                HorizontalGuide2.X2 = e.NewSize.Width;
            };
        }
        private void UpdateGuideVisibility()
        {
            HideAllGuides();

            double currentX = ViewModel.PositionX;
            double currentY = ViewModel.PositionY;
            double width = DraggableBorder.ActualWidth;
            double height = DraggableBorder.ActualHeight;

            // 检查当前（已吸附）的位置是否正好在辅助线上
            var borderEdgesX = new[] { currentX, currentX + width / 2, currentX + width };
            foreach (double guideX in _verticalGuides)
            {
                if (Array.Exists(borderEdgesX, edge => Math.Abs(edge - guideX) < 1.0)) // 用一个很小的容差
                {
                    ShowGuide(isVertical: true, index: _verticalGuides.IndexOf(guideX));
                }
            }

            // ... 对水平辅助线做同样的操作 ...
            var borderEdgesY = new[] { currentY, currentY + height / 2, currentY + height };
            foreach (double guideY in _horizontalGuides)
            {
                if (Array.Exists(borderEdgesY, edge => Math.Abs(edge - guideY) < 1.0))
                {
                    ShowGuide(isVertical: false, index: _horizontalGuides.IndexOf(guideY));
                }
            }
        }
        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // 添加光标变化
        }
        //private void UpdateSnappingAndGuides(double newX, double newY)
        //{
        //    // 这个方法接收一个理论坐标，然后检查吸附并更新辅助线
        //    // 注意：这里我们不再直接设置 Canvas.Left/Top，因为数据绑定会处理
        //    var snappedPosition = CheckAndApplySnapping(newX, newY, DraggableBorder.ActualWidth, DraggableBorder.ActualHeight);

        //    // 你仍然可以在这里直接更新UI，但更推荐的方式是让ViewModel驱动
        //    // 不过为了最小化改动，我们暂时保留CheckAndApplySnapping中的UI更新
        //}
        private void Border_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            ViewModel.IsDragging = true;
            // 记录指针相对于Border左上角的偏移
            var startPoint = e.GetCurrentPoint(RootCanvas).Position;
            _pointerOffset = new Point(startPoint.X - Canvas.GetLeft(border), startPoint.Y - Canvas.GetTop(border));

            border.CapturePointer(e.Pointer);
        }

        private void Border_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (ViewModel.IsDragging)
            {
                var border = sender as FrameworkElement;
                var currentPointerPosition = e.GetCurrentPoint(RootCanvas).Position;
                double newX = currentPointerPosition.X - _pointerOffset.X;
                double newY = currentPointerPosition.Y - _pointerOffset.Y;

                // ★★★★★ 核心修改 2: 调用 ViewModel 来处理一切 ★★★★★
                ViewModel.UpdatePosition(newX, newY, border.ActualWidth, border.ActualHeight);
            }
        }

        // 拖动结束后，确保所有辅助线都隐藏了
        private void Border_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ViewModel.IsDragging = false;
            var border = sender as Border;
            border?.ReleasePointerCapture(e.Pointer);

            // 拖动结束后隐藏所有辅助线
            HideAllGuides();
        }
        //private Point CheckAndApplySnapping(double newX, double newY, double width, double height)
        //{
        //    double finalX = newX;
        //    double finalY = newY;

        //    // 先隐藏所有辅助线
        //    HideAllGuides();

        //    // --- 检查垂直吸附 ---
        //    var borderEdgesX = new[] { newX, newX + width / 2, newX + width }; // 左、中、右
        //    foreach (double guideX in _verticalGuides)
        //    {
        //        // 左边缘
        //        if (Math.Abs(borderEdgesX[0] - guideX) < SnapThreshold)
        //        {
        //            finalX = guideX;
        //            ShowGuide(isVertical: true, index: _verticalGuides.IndexOf(guideX));
        //            break; // 一旦吸附，就不再检查其他垂直线
        //        }
        //        // 右边缘
        //        if (Math.Abs(borderEdgesX[2] - guideX) < SnapThreshold)
        //        {
        //            finalX = guideX - width;
        //            ShowGuide(isVertical: true, index: _verticalGuides.IndexOf(guideX));
        //            break;
        //        }
        //        // 中心
        //        if (Math.Abs(borderEdgesX[1] - guideX) < SnapThreshold)
        //        {
        //            finalX = guideX - width / 2;
        //            ShowGuide(isVertical: true, index: _verticalGuides.IndexOf(guideX));
        //            break;
        //        }
        //    }

        //    // --- 检查水平吸附 ---
        //    var borderEdgesY = new[] { newY, newY + height / 2, newY + height }; // 上、中、下
        //    foreach (double guideY in _horizontalGuides)
        //    {
        //        // 上边缘
        //        if (Math.Abs(borderEdgesY[0] - guideY) < SnapThreshold)
        //        {
        //            finalY = guideY;
        //            ShowGuide(isVertical: false, index: _horizontalGuides.IndexOf(guideY));
        //            break;
        //        }
        //        // 下边缘
        //        if (Math.Abs(borderEdgesY[2] - guideY) < SnapThreshold)
        //        {
        //            finalY = guideY - height;
        //            ShowGuide(isVertical: false, index: _horizontalGuides.IndexOf(guideY));
        //            break;
        //        }
        //        // 中心
        //        if (Math.Abs(borderEdgesY[1] - guideY) < SnapThreshold)
        //        {
        //            finalY = guideY - height / 2;
        //            ShowGuide(isVertical: false, index: _horizontalGuides.IndexOf(guideY));
        //            break;
        //        }
        //    }

        //    return new Point(finalX, finalY);
        //}

        private void HideAllGuides()
        {
            VerticalGuide1.Visibility = Visibility.Collapsed;
            VerticalGuide2.Visibility = Visibility.Collapsed;
            HorizontalGuide1.Visibility = Visibility.Collapsed;
            HorizontalGuide2.Visibility = Visibility.Collapsed;
        }

        private void ShowGuide(bool isVertical, int index)
        {
            if (isVertical)
            {
                if (index == 0) VerticalGuide1.Visibility = Visibility.Visible;
                if (index == 1) VerticalGuide2.Visibility = Visibility.Visible;
            }
            else
            {
                if (index == 0) HorizontalGuide1.Visibility = Visibility.Visible;
                if (index == 1) HorizontalGuide2.Visibility = Visibility.Visible;
            }
        }
    }
}