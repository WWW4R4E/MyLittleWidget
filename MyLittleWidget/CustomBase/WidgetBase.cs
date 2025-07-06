using Microsoft.UI.Xaml.Input;
using MyLittleWidget.ViewModels;

namespace MyLittleWidget.CustomBase
{
    public partial class WidgetBase : ContentControl
    {
        private bool _isDragging;
        private Point _pointerOffset;
        private Canvas _parentCanvas;

        #region Events and Handlers
        public event EventHandler PositionUpdated;
        public event EventHandler DragStarted;
        public event EventHandler DragCompleted;
        public Func<Point, Size, Point> SnappingHandler { get; set; }
        #endregion

        #region Configuration and Initialization
        internal WidgetConfig Config { get; set; }
        public virtual void Initialize()
        {
            if (this.Config != null)
            {
                this.Config.PropertyChanged -= OnConfigPropertyChanged;
            }
            //this.Config = config;
            this.Config.PropertyChanged += OnConfigPropertyChanged;
            UpdatePositionFromConfig();
        }
        #endregion
        private void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // 在 UI 线程上更新
            DispatcherQueue.TryEnqueue(() =>
            {
                if (e.PropertyName == nameof(WidgetConfig.PositionX) ||
                    e.PropertyName == nameof(WidgetConfig.PositionY))
                {
                    UpdatePositionFromConfig();
                }
            });
        }

        private void UpdatePositionFromConfig()
        {
            if (Config == null) return;

            // 这就是你之前 OnPositionChanged 回调里做的事情
            Canvas.SetLeft(this, Config.PositionX);
            Canvas.SetTop(this, Config.PositionY);
            PositionUpdated?.Invoke(this, EventArgs.Empty);
        }
        public WidgetBase()
        {
            this.DefaultStyleKey = typeof(WidgetBase);
            // 默认样式
            this.Content = new Border
            {
                HorizontalAlignment =HorizontalAlignment.Stretch,
                VerticalAlignment =VerticalAlignment.Stretch,
                CornerRadius = new CornerRadius(8),
            };
            // 绑定事件 (这是拖动逻辑的核心)
            this.Loaded += OnWidgetLoaded;
            this.PointerPressed += OnWidgetPointerPressed;
            this.PointerMoved += OnWidgetPointerMoved;
            this.PointerReleased += OnWidgetPointerReleased;
            this.PointerCanceled += OnWidgetPointerReleased;
            this.PointerCaptureLost += OnWidgetPointerReleased;
            this.Unloaded += OnWidgetUnloaded;
            SetupContextMenu();

        }

        private void OnWidgetLoaded(object sender, RoutedEventArgs e)
        {
            // 订阅全局设置的变化
            AppSettings.Instance.PropertyChanged += OnAppSettingsChanged;

            UpdateTheme(AppSettings.Instance.IsDarkTheme);
            UpdateSize(AppSettings.Instance.BaseUnit);

            _parentCanvas = VisualTreeHelper.GetParent(this) as Canvas;
        }

        private void OnWidgetUnloaded(object sender, RoutedEventArgs e)
        {
            AppSettings.Instance.PropertyChanged -= OnAppSettingsChanged;

            if (this.Config != null)
            {
                this.Config.PropertyChanged -= OnConfigPropertyChanged;
            }
        }

        private void OnAppSettingsChanged(object? sender, PropertyChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (e.PropertyName == nameof(AppSettings.Instance.BaseUnit))
                {
                    UpdateSize(AppSettings.Instance.BaseUnit);
                }
                else if (e.PropertyName == nameof(AppSettings.Instance.IsDarkTheme))
                {
                    UpdateTheme(AppSettings.Instance.IsDarkTheme);
                }
            });
        }

        private void UpdateSize(double newBaseUnit)
        {
            this.Width = Config.UnitWidth * newBaseUnit;
            this.Height = Config.UnitHeight * newBaseUnit;
        }

        // 更新主题的逻辑
        private void UpdateTheme(bool isDark)
        {
            // 示例：根据主题改变背景色
            // 注意：这里只是一个简单示例，真实项目中你可能会使用资源字典和主题绑定
            if (this.Content is Border border)
            {
                border.Background = new SolidColorBrush(isDark ? Colors.DarkSlateGray : Colors.LightGray);
            }
        }

        private void OnWidgetPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_parentCanvas == null) return;

            _isDragging = true;
            var startPoint = e.GetCurrentPoint(_parentCanvas).Position;
            _pointerOffset = new Point(startPoint.X - this.Config.PositionX, startPoint.Y - this.Config.PositionY);

            this.CapturePointer(e.Pointer);
            DragStarted?.Invoke(this, EventArgs.Empty);

            // 将此组件置于顶层
            if (VisualTreeHelper.GetParent(this) is Canvas)
            {
                Canvas.SetZIndex(this, 99);
            }
            e.Handled = true; // 阻止事件冒泡，以免触发父容器的其他行为
        }

        private void OnWidgetPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isDragging && _parentCanvas != null)
            {
                var currentPointerPosition = e.GetCurrentPoint(_parentCanvas).Position;
                double newX = currentPointerPosition.X - _pointerOffset.X;
                double newY = currentPointerPosition.Y - _pointerOffset.Y;

                if (SnappingHandler != null)
                {
                    var snappedPosition = SnappingHandler(new Point(newX, newY), new Size(this.ActualWidth, this.ActualHeight));
                    this.Config.PositionX = snappedPosition.X;
                    this.Config.PositionY = snappedPosition.Y;
                }
                else
                {
                    this.Config.PositionX = newX;
                    this.Config.PositionY = newY;
                }
            }
        }

        private void OnWidgetPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                this.ReleasePointerCapture(e.Pointer);
                DragCompleted?.Invoke(this, EventArgs.Empty);

                if (VisualTreeHelper.GetParent(this) is Canvas)
                {
                    Canvas.SetZIndex(this, 0);
                }
            }
        }

        // 6. 你原来的其他方法 (保持不变)
        protected virtual void OnSettingsChanged()
        {
            // ...
        }

        private void SetupContextMenu()
        {
            // 实现通用右键菜单的代码...
        }
    }
}