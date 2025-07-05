using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Shapes;
using MyLittleWidget.Custom;
using MyLittleWidget.CustomBase;
using MyLittleWidget.Utils;
using MyLittleWidget.ViewModels;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.System;

namespace MyLittleWidget.Views.Pages
{

    public sealed partial class DocklinePage : Page
    {
        public SharedViewModel ViewModel { get; } = SharedViewModel.Instance;
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

            EmbedIntoTargetWindow();

            // 创建辅助线
            var LineInfo = GetDesktop.GetDesktopGridInfo();
            var dpiScale = XamlRoot.RasterizationScale;
            SetupGuideLines(vLineCount: LineInfo.grid.X, vLineSpacing: LineInfo.spacing.X / ViewModel.Scale / 1.5, hLineCount: LineInfo.grid.Y, hLineSpacing: LineInfo.spacing.Y / ViewModel.Scale / 1.5);

            SharedViewModel.Instance.ConfigureGuides(_vGuideCoordinates, _hGuideCoordinates);

            var widget1 = new TestWidget { PositionX = 100, PositionY = 100, };
            var widget2 = new TestWidget { PositionX = 400, PositionY = 200, };

            var widgets = new ObservableCollection<WidgetBase> { widget1, widget2 };

            ViewModel.ConfigureWidget(widgets);
            foreach (var widget in widgets)
            {
                RootCanvas.Children.Add(widget);
                widget.PositionUpdated += OnWidgetPositionUpdated;
            }
            ViewModel.PropertyChanged += ViewModel_PropertyChanged_ForGuideVisibility;
        }
        private void EmbedIntoTargetWindow()
        {
            var workArea = GetDesktop.GetDesktopGridInfo().rcWorkArea;
            var childWindow = ((App)App.Current).childWindow;
            childWindow.AppWindow.MoveAndResize(new RectInt32(
                workArea.X,
                workArea.Y,
                workArea.Width,
                workArea.Height
            ));
            HWND myHwnd = (HWND)WindowNative.GetWindowHandle(childWindow);
            childWindow. AppWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
            var presenter = childWindow. AppWindow.Presenter as OverlappedPresenter;

            if (presenter != null)
            {
                presenter.SetBorderAndTitleBar(false, false);
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
            }
            childWindow.AppWindow.MoveAndResize(new RectInt32(
                workArea.X,
                workArea.Y,
                workArea.Width,
                workArea.Height
            ));
            HWND progman = PInvoke.FindWindow("Progman", null);
            HWND workerw = PInvoke.FindWindowEx(progman, HWND.Null, "WorkerW", null);

            PInvoke.SetParent(myHwnd, workerw);
            childWindow.Activate();

        }
        private void OnWidgetPositionUpdated(object sender, EventArgs e)
        {
            if (sender is WidgetBase widget && ViewModel.IsDragging && ViewModel.ActiveWidget == widget)
            {
                UpdateGuideVisibility(widget);
            }
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
            for (int i = 0; i <= vLineCount; i++)
            {
                double xPos = i * vLineSpacing;
                _vGuideCoordinates.Add(xPos);
            }

            // 水平线的坐标
            for (int i = 0; i <= hLineCount; i++)
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
#if DEBUG
                    Visibility = Visibility.Visible
#else
     Visibility = Visibility.Collapsed
#endif
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
#if DEBUG
                    Visibility = Visibility.Visible
#else
     Visibility = Visibility.Collapsed
#endif
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
                // 在垂直坐标列表中这个坐标的索引
                int index = _vGuideCoordinates.IndexOf(coordinate);
                if (index != -1 && index < _vGuideLines.Count)
                {
                    _vGuideLines[index].Visibility = Visibility.Visible;
                }
            }
            else
            {
                // 在水平坐标列表中这个坐标的索引
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
#if RELEASE
            
            foreach (var line in _vGuideLines) line.Visibility = Visibility.Collapsed;
            foreach (var line in _hGuideLines) line.Visibility = Visibility.Collapsed;
#endif
        }
    }
}