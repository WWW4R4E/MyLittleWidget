using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Shapes;
using MyLittleWidget.Utils;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyLittleWidget.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GridContainerWindow : Window
    {
        const uint WS_CHILD = 0x40000000;

        // --- 可配置参数 ---
        private const int GridColumns = 10; // 垂直线的数量
        private const int GridRows = 8;     // 水平线的数量
        private const double HighlightThreshold = 15.0; // 靠近多远时高亮 (像素)
        private const double SnapThreshold = 8.0;       // 靠近多远时吸附 (像素)

        // --- 线条样式 ---
        private readonly SolidColorBrush _defaultLineBrush = new SolidColorBrush(Colors.Gray);
        private readonly SolidColorBrush _highlightLineBrush = new SolidColorBrush(Colors.DodgerBlue);
        private const double DefaultLineThickness = 1.0;
        private const double HighlightLineThickness = 2.5;

        // --- 内部状态 ---
        private List<Line> _verticalLines = new List<Line>();
        private List<Line> _horizontalLines = new List<Line>();
        private bool _isSnapping = false; // 防止吸附时触发的事件造成递归调用


        GetDesktop.RECT workersize;

        internal GridContainerWindow(GetDesktop.RECT workersize)
        {
           this.workersize = workersize;
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;

            SystemBackdrop = new TransparentBackdrop();
            SetupWindow();

        }

        private void SetupWindow()
        {
            // 设置窗口大小为工作区大小
            this.AppWindow.MoveAndResize(new RectInt32
            {
                X = workersize.Left,
                Y = workersize.Top,
                Width = workersize.Right - workersize.Left,
                Height = workersize.Bottom - workersize.Top
            });


            HWND childHwnd = (HWND)WindowNative.GetWindowHandle(this);
            PInvoke.SetWindowLong(childHwnd, Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)WS_CHILD);
            HWND hProgman = PInvoke.FindWindow("Progman", null);
            HWND hWorkw = PInvoke.FindWindowEx(hProgman, HWND.Null, "WorkerW", null);


            // 设置当前窗口为桌面视图的子窗口
            PInvoke.SetParent(childHwnd, hWorkw);
            Debug.WriteLine($"设置窗口 {childHwnd} 为桌面视图的子窗口 {hWorkw}");
        }
        private void CreateGridLines()
        {
            // 清理旧的线条
            GridCanvas.Children.Clear();
            _verticalLines.Clear();
            _horizontalLines.Clear();

            // 获取当前窗口的实际尺寸
            double width = this.Content.ActualSize.X;
            double height = this.Content.ActualSize.Y;

            if (width == 0 || height == 0) return;

            // 计算间距
            double colSpacing = width / (GridColumns + 1);
            double rowSpacing = height / (GridRows + 1);

            // 创建垂直线
            for (int i = 1; i <= GridColumns; i++)
            {
                var line = new Line
                {
                    X1 = i * colSpacing,
                    Y1 = 0,
                    X2 = i * colSpacing,
                    Y2 = height,
                    Stroke = _defaultLineBrush,
                    StrokeThickness = DefaultLineThickness
                };
                _verticalLines.Add(line);
                GridCanvas.Children.Add(line);
            }

            // 创建水平线
            for (int i = 1; i <= GridRows; i++)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = i * rowSpacing,
                    X2 = width,
                    Y2 = i * rowSpacing,
                    Stroke = _defaultLineBrush,
                    StrokeThickness = DefaultLineThickness
                };
                _horizontalLines.Add(line);
                GridCanvas.Children.Add(line);
            }
        }

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isSnapping) return; // 如果正在吸附，则忽略此次移动事件

            // 获取鼠标相对于Canvas的位置
            var currentPoint = e.GetCurrentPoint(GridCanvas).Position;

            // --- 1. 高亮逻辑 ---
            ResetAllLinesStyle();
            Line closestVertical = null;
            Line closestHorizontal = null;
            double minXDist = double.MaxValue;
            double minYDist = double.MaxValue;

            // 查找最近的垂直线
            foreach (var line in _verticalLines)
            {
                double dist = Math.Abs(line.X1 - currentPoint.X);
                if (dist < minXDist)
                {
                    minXDist = dist;
                    closestVertical = line;
                }
            }

            // 查找最近的水平线
            foreach (var line in _horizontalLines)
            {
                double dist = Math.Abs(line.Y1 - currentPoint.Y);
                if (dist < minYDist)
                {
                    minYDist = dist;
                    closestHorizontal = line;
                }
            }

            // 如果足够近，则高亮
            if (closestVertical != null && minXDist < HighlightThreshold)
            {
                HighlightLine(closestVertical);
            }
            if (closestHorizontal != null && minYDist < HighlightThreshold)
            {
                HighlightLine(closestHorizontal);
            }

            // --- 2. 吸附逻辑 ---
            NativeMethods.GetCursorPos(out var screenPoint);
            int snapToX = screenPoint.X;
            int snapToY = screenPoint.Y;
            bool shouldSnap = false;

            // 检查是否需要垂直吸附
            if (closestVertical != null && minXDist < SnapThreshold)
            {
                var hwnd = WindowNative.GetWindowHandle(this);
                var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);
                snapToX = appWindow.Position.X + (int)closestVertical.X1;
                shouldSnap = true;
            }

            // 检查是否需要水平吸附
            if (closestHorizontal != null && minYDist < SnapThreshold)
            {
                var hwnd = WindowNative.GetWindowHandle(this);
                var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);
                snapToY = appWindow.Position.Y + (int)closestHorizontal.Y1;
                shouldSnap = true;
            }

            // 如果需要吸附且位置已改变，则移动鼠标
            if (shouldSnap && (snapToX != screenPoint.X || snapToY != screenPoint.Y))
            {
                _isSnapping = true;
                NativeMethods.SetCursorPos(snapToX, snapToY);
                _isSnapping = false;
            }
        }

        private void HighlightLine(Line line)
        {
            line.Stroke = _highlightLineBrush;
            line.StrokeThickness = HighlightLineThickness;
            Canvas.SetZIndex(line, 1); // 确保高亮线在最上层
        }

        private void ResetAllLinesStyle()
        {
            Debug.Write(123);
            foreach (var line in _verticalLines)
            {
                line.Stroke = _defaultLineBrush;
                line.StrokeThickness = DefaultLineThickness;
                Canvas.SetZIndex(line, 0);
            }
            foreach (var line in _horizontalLines)
            {
                line.Stroke = _defaultLineBrush;
                line.StrokeThickness = DefaultLineThickness;
                Canvas.SetZIndex(line, 0);
                Debug.Write(123);
            }
        }

        // 当窗口首次激活时，创建网格线
        private void Window_Activated()
        {
            Debug.WriteLine(123);

            if (GridCanvas.Children.Count == 0)
            {
                CreateGridLines();
            }
        }


        // 当窗口大小改变时（例如移动到不同分辨率的显示器），重新创建网格
        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            CreateGridLines();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window_Activated();
        }
    }

    // --- Win32 P/Invoke 帮助类 ---
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;
            public int Y;
        }
    }
}