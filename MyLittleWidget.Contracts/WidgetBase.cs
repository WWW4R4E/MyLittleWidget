using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.ComponentModel;
using Windows.Foundation;

namespace MyLittleWidget.Contracts
{
  public partial class WidgetBase : ContentControl
  {
    protected IApplicationSettings AppSettings { get; }

    private bool _isDragging;
    private Point _pointerOffset;
    private Canvas _parentCanvas;

    #region Events and Handlers

    public event EventHandler PositionUpdated;

    public event EventHandler DragStarted;

    public event EventHandler DragCompleted;

    public Func<Point, Size, Point> SnappingHandler { get; set; }

    #endregion Events and Handlers

    #region Configuration and Initialization

    public WidgetConfig Config { get; }

    #endregion Configuration and Initialization

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

    // 提供给子类重写的配置方法
    protected virtual void ConfigureWidget()
    {
    }
    public WidgetBase(WidgetConfig config, IApplicationSettings settings)
    {
      this.DefaultStyleKey = typeof(WidgetBase);
      this.AppSettings = settings ?? throw new ArgumentNullException(nameof(settings));
      // 默认样式
      this.Content = new Border
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
        CornerRadius = new CornerRadius(8),
      };
      this.Config = config ?? throw new ArgumentNullException(nameof(config));
      // 子类通过这个配置Config对象
      ConfigureWidget(); 
      this.Config.PropertyChanged += OnConfigPropertyChanged;
      UpdatePositionFromConfig();
      // 绑定事件 
      this.Loaded += OnWidgetLoaded;
      //this.PointerPressed += OnWidgetPointerPressed;
      //this.PointerMoved += OnWidgetPointerMoved;
      //this.PointerReleased += OnWidgetPointerReleased;
      //this.PointerCanceled += OnWidgetPointerReleased;
      //this.PointerCaptureLost += OnWidgetPointerReleased;
      this.Unloaded += OnWidgetUnloaded;
      SetupContextMenu();
    }
    // 无参数的构造函数给XAML设计器用
    //[Obsolete("This constructor is for XAML designer support only.", true)]
    //public WidgetBase() : this(new WidgetConfig()) { }
    private void OnWidgetLoaded(object sender, RoutedEventArgs e)
    {
      // 订阅全局设置的变化
      AppSettings.PropertyChanged += OnAppSettingsChanged;

      UpdateTheme(AppSettings.IsDarkTheme);
      UpdateSize(AppSettings.BaseUnit);

      _parentCanvas = VisualTreeHelper.GetParent(this) as Canvas;
    }

    private void OnWidgetUnloaded(object sender, RoutedEventArgs e)
    {
      AppSettings.PropertyChanged -= OnAppSettingsChanged;

      if (this.Config != null)
      {
        this.Config.PropertyChanged -= OnConfigPropertyChanged;
      }
    }

    private void OnAppSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
      DispatcherQueue.TryEnqueue(() =>
      {
        if (e.PropertyName == nameof(AppSettings.BaseUnit))
        {
          UpdateSize(AppSettings.BaseUnit);
        }
        else if (e.PropertyName == nameof(AppSettings.IsDarkTheme))
        {
          UpdateTheme(AppSettings.IsDarkTheme);
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
      // TODO : 完善更新逻辑
      if (this.Content is Border border)
      {
        border.Background = new SolidColorBrush(isDark ? Colors.DarkSlateGray : Colors.LightGray);
      }
    }

    //private void OnWidgetPointerPressed(object sender, PointerRoutedEventArgs e)
    //{
    //  if (_parentCanvas == null) return;

    //  _isDragging = true;
    //  var startPoint = e.GetCurrentPoint(_parentCanvas).Position;
    //  _pointerOffset = new Point(startPoint.X - this.Config.PositionX, startPoint.Y - this.Config.PositionY);

    //  this.CapturePointer(e.Pointer);
    //  DragStarted?.Invoke(this, EventArgs.Empty);

    //  // 将此组件置于顶层
    //  if (VisualTreeHelper.GetParent(this) is Canvas)
    //  {
    //    Canvas.SetZIndex(this, 99);
    //  }
    //  e.Handled = true; // 阻止事件冒泡，以免触发父容器的其他行为
    //}

    //private void OnWidgetPointerMoved(object sender, PointerRoutedEventArgs e)
    //{
    //  if (_isDragging && _parentCanvas != null)
    //  {
    //    var currentPointerPosition = e.GetCurrentPoint(_parentCanvas).Position;
    //    double newX = currentPointerPosition.X - _pointerOffset.X;
    //    double newY = currentPointerPosition.Y - _pointerOffset.Y;

    //    if (SnappingHandler != null)
    //    {
    //      var snappedPosition = SnappingHandler(new Point(newX, newY), new Size(this.ActualWidth, this.ActualHeight));
    //      this.Config.PositionX = snappedPosition.X;
    //      this.Config.PositionY = snappedPosition.Y;
    //    }
    //    else
    //    {
    //      this.Config.PositionX = newX;
    //      this.Config.PositionY = newY;
    //    }
    //  }
    //}

    //private void OnWidgetPointerReleased(object sender, PointerRoutedEventArgs e)
    //{
    //  if (_isDragging)
    //  {
    //    _isDragging = false;
    //    this.ReleasePointerCapture(e.Pointer);
    //    DragCompleted?.Invoke(this, EventArgs.Empty);

    //    if (VisualTreeHelper.GetParent(this) is Canvas)
    //    {
    //      Canvas.SetZIndex(this, 0);
    //    }
    //  }
    //}

    // 你的其他方法
    protected virtual void OnSettingsChanged()
    {
    }
    // 实现通用右键菜单的代码...
    private void SetupContextMenu()
    {
    }
  }
}