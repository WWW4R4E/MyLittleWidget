using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Shapes;
using MyLittleWidget.Custom;
using MyLittleWidget.CustomBase;
using MyLittleWidget.ViewModels;

namespace MyLittleWidget.Views.Pages
{

    public sealed partial class DocklinePage : Page
    {
        public SharedViewModel ViewModel { get; } = SharedViewModel.Instance;
        //// 指针相对于 Border 左上角的偏移
        //private Point _pointerOffset;

        // --- 吸附逻辑相关 ---
        // 吸附的距离阈值，当边缘距离辅助线小于这个值时，触发吸附
        private const double SnapThreshold = 10.0;
        // 垂直和水平辅助线
        private readonly List<double> _vGuideCoordinates = new List<double>();
        private readonly List<double> _hGuideCoordinates = new List<double>();
        private readonly List<Line> _vGuideLines = new List<Line>();
        private readonly List<Line> _hGuideLines = new List<Line>();

        public DocklinePage()
        {
            InitializeComponent();
            this.Loaded += OnPageLoaded;

        }
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            // 创建辅助线
            SetupGuideLines(vLineCount: 3, vLineSpacing: 200, hLineCount: 2, hLineSpacing: 150);

            SharedViewModel.Instance.ConfigureGuides(_vGuideCoordinates, _hGuideCoordinates);

            // 创建组件
            var widget1 = new TestWidget { PositionX = 100, PositionY = 100, };
            var widget2 = new TestWidget { PositionX = 400, PositionY = 200, };

            var widgets = new ObservableCollection<WidgetBase> { widget1, widget2 };

            // 将组件列表配置到ViewModel中
            ViewModel.ConfigureWidget(widgets);
            foreach (var widget in widgets)
            {
                RootCanvas.Children.Add(widget);
            }
            ViewModel.PropertyChanged += ViewModel_PropertyChanged_ForGuideVisibility;
        }

        private void ViewModel_PropertyChanged_ForGuideVisibility(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.IsDragging) || e.PropertyName == nameof(ViewModel.ActiveWidget))
            {
                if (ViewModel.IsDragging && ViewModel.ActiveWidget != null)
                {
                    UpdateGuideVisibility(ViewModel.ActiveWidget);
                }
                else
                {
                    HideAllGuides();
                }
            }
        }
        private void SetupGuideLines(int vLineCount, double vLineSpacing, int hLineCount, double hLineSpacing)
        {
            // 垂直线的坐标
            for (int i = 1; i <= vLineCount; i++)
            {
                double xPos = i * vLineSpacing;
                _vGuideCoordinates.Add(xPos); 
            }

            // 水平线的坐标
            for (int i = 1; i <= hLineCount; i++)
            {
                double yPos = i * hLineSpacing;
                _hGuideCoordinates.Add(yPos);
            }

            foreach (double xPos in _vGuideCoordinates)
            {
                var guideLine = new Line
                {
                    Stroke = new SolidColorBrush(Colors.DodgerBlue),
                    StrokeDashArray = new DoubleCollection { 4, 2 },
                    StrokeThickness = 1,
                    Visibility = Visibility.Collapsed
                };
                Canvas.SetLeft(guideLine, xPos);
                guideLine.Y1 = 0;
                guideLine.Y2 = RootCanvas.ActualHeight;
                RootCanvas.Children.Add(guideLine);
                _vGuideLines.Add(guideLine);
            }

            foreach (double yPos in _hGuideCoordinates)
            {
                var guideLine = new Line
                {
                    Stroke = new SolidColorBrush(Colors.DodgerBlue),
                    StrokeDashArray = new DoubleCollection { 4, 2 },
                    StrokeThickness = 1,
                    Visibility = Visibility.Collapsed
                };
                Canvas.SetTop(guideLine, yPos);
                guideLine.X1 = 0;
                guideLine.X2 = RootCanvas.ActualWidth;
                RootCanvas.Children.Add(guideLine);
                _hGuideLines.Add(guideLine);
            }

            RootCanvas.SizeChanged += (s, e) =>
            {
                foreach (var line in _vGuideLines) line.Y2 = e.NewSize.Height;
                foreach (var line in _hGuideLines) line.X2 = e.NewSize.Width;
            };
        }
        private void ShowGuide(bool isVertical, double coordinate)
        {
            if (isVertical)
            {
                // 在垂直坐标列表中找到这个坐标的索引
                int index = _vGuideCoordinates.IndexOf(coordinate);
                if (index != -1 && index < _vGuideLines.Count)
                {
                    _vGuideLines[index].Visibility = Visibility.Visible;
                }
            }
            else
            {
                // 在水平坐标列表中找到这个坐标的索引
                int index = _hGuideCoordinates.IndexOf(coordinate);
                if (index != -1 && index < _hGuideLines.Count)
                {
                    _hGuideLines[index].Visibility = Visibility.Visible;
                }
            }
        }
        private void UpdateGuideVisibility(WidgetBase widget)
        {
            HideAllGuides();
            if (widget == null) return;

            double currentX = widget.PositionX;
            double currentY = widget.PositionY;
            double width = widget.ActualWidth;
            double height = widget.ActualHeight;

            var borderEdgesX = new[] { currentX, currentX + width / 2, currentX + width };
            foreach (double guideX in _vGuideCoordinates)
            {
                if (Array.Exists(borderEdgesX, edge => Math.Abs(edge - guideX) < 1.0))
                {
                    ShowGuide(isVertical: true, coordinate: guideX);
                }
            }

            var borderEdgesY = new[] { currentY, currentY + height / 2, currentY + height };
            foreach (double guideY in _hGuideCoordinates)
            {
                if (Array.Exists(borderEdgesY, edge => Math.Abs(edge - guideY) < 1.0))
                {
                    ShowGuide(isVertical: false, coordinate: guideY);
                }
            }
        }
        private void HideAllGuides()
        {
            foreach (var line in _vGuideLines) line.Visibility = Visibility.Collapsed;
            foreach (var line in _hGuideLines) line.Visibility = Visibility.Collapsed;
        }

        //private void Border_PointerPressed(object sender, PointerRoutedEventArgs e)
        //{
        //    var border = sender as Border;
        //    if (border == null) return;

        //    Instance.IsDragging = true;
        //    // 记录指针相对于Border左上角的偏移
        //    var startPoint = e.GetCurrentPoint(RootCanvas).Position;
        //    _pointerOffset = new Point(startPoint.X - Canvas.GetLeft(border), startPoint.Y - Canvas.GetTop(border));

        //    border.CapturePointer(e.Pointer);
        //}

        //private void Border_PointerMoved(object sender, PointerRoutedEventArgs e)
        //{
        //    if (Instance.IsDragging)
        //    {
        //        var border = sender as FrameworkElement;
        //        var currentPointerPosition = e.GetCurrentPoint(RootCanvas).Position;
        //        double newX = currentPointerPosition.X - _pointerOffset.X;
        //        double newY = currentPointerPosition.Y - _pointerOffset.Y;

        //        Instance.UpdateActiveWidgetPosition(newX, newY);
        //    }
        //}

        //// 拖动结束后，确保所有辅助线都隐藏了
        //private void Border_PointerReleased(object sender, PointerRoutedEventArgs e)
        //{
        //    ViewModel.IsDragging = false;
        //    var border = sender as Border;
        //    border?.ReleasePointerCapture(e.Pointer);

        //    HideAllGuides();
        //}

      
    }
}