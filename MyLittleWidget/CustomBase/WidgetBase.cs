using Microsoft.UI.Xaml.Input;
using MyLittleWidget.ViewModels;

namespace MyLittleWidget.Custom
{
    public abstract class WidgetBase : ContentControl
    {
        private readonly SharedViewModel _viewModel = SharedViewModel.Instance;
        public Guid Id { get; } = Guid.NewGuid();

        #region 依赖属性：存储状态 (Position, Size, etc.)

        public static readonly DependencyProperty PositionXProperty =
            DependencyProperty.Register(nameof(PositionX), typeof(double), typeof(WidgetBase), new PropertyMetadata(0.0, OnPositionChanged));

        public double PositionX
        {
            get { return (double)GetValue(PositionXProperty); }
            set { SetValue(PositionXProperty, value); }
        }

        public static readonly DependencyProperty PositionYProperty =
            DependencyProperty.Register(nameof(PositionY), typeof(double), typeof(WidgetBase), new PropertyMetadata(0.0, OnPositionChanged));

        public double PositionY
        {
            get { return (double)GetValue(PositionYProperty); }
            set { SetValue(PositionYProperty, value); }
        }

        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WidgetBase widget)
            {
                Canvas.SetLeft(widget, widget.PositionX);
                Canvas.SetTop(widget, widget.PositionY);
                widget.PositionUpdated?.Invoke(widget, EventArgs.Empty);
            }
        }
        #endregion
        #region 新增：吸附逻辑的回调
        public Func<Point, Size, Point> SnappingHandler { get; set; }
        #endregion
        #region 事件：用于通知外部
        public event EventHandler PositionUpdated;
        public event EventHandler DragStarted;
        public event EventHandler DragCompleted;
        #endregion

        #region 内部拖动逻辑 (对组件开发者隐藏)

        private bool _isDragging;
        private Point _pointerOffset;
        private Canvas _parentCanvas; // 用来获取父Canvas的引用

        protected WidgetBase()
        {
            // 默认样式
            this.Content = new Border
            {
                CornerRadius = new CornerRadius(8),
                Width = 200,
                Height = 200,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // 绑定事件
            this.Loaded += OnWidgetLoaded;
            this.PointerPressed += OnWidgetPointerPressed;
            this.PointerMoved += OnWidgetPointerMoved;
            this.PointerReleased += OnWidgetPointerReleased;
            this.PointerCanceled += OnWidgetPointerReleased; 
            this.PointerCaptureLost += OnWidgetPointerReleased; 
        }

        private void OnWidgetLoaded(object sender, RoutedEventArgs e)
        {
            // 控件加载后，尝试获取父Canvas的引用
            _parentCanvas = VisualTreeHelper.GetParent(this) as Canvas;
        }

        private void OnWidgetPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_parentCanvas == null) return;

            _isDragging = true;
            var startPoint = e.GetCurrentPoint(_parentCanvas).Position;
            _pointerOffset = new Point(startPoint.X - this.PositionX, startPoint.Y - this.PositionY);

            this.CapturePointer(e.Pointer);
            DragStarted?.Invoke(this, EventArgs.Empty);

            // 将此组件置于顶层（如果Canvas中有多个组件）
            Canvas.SetZIndex(this, 99);
        }

        private void OnWidgetPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isDragging && _parentCanvas != null)
            {
                var currentPointerPosition = e.GetCurrentPoint(_parentCanvas).Position;
                double newX = currentPointerPosition.X - _pointerOffset.X;
                double newY = currentPointerPosition.Y - _pointerOffset.Y;
                // 吸附处理
                if (SnappingHandler != null)
                {
                    
                    var snappedPosition = SnappingHandler(new Point(newX, newY), new Size(this.ActualWidth, this.ActualHeight));
                    this.PositionX = snappedPosition.X;
                    this.PositionY = snappedPosition.Y;
                }
                else
                {
                    this.PositionX = newX;
                    this.PositionY = newY;
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
                Canvas.SetZIndex(this, 0); 
            }
        }
        #endregion
    }
}
