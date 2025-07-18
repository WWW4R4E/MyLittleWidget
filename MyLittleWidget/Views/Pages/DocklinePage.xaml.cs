using Microsoft.UI.Xaml.Shapes;
using MyLittleWidget.Contracts;
using MyLittleWidget.Models;
using MyLittleWidget.Services;
using MyLittleWidget.Utils;
using MyLittleWidget.Utils.BackDrop;
using MyLittleWidget.ViewModels;
using System.Diagnostics;

namespace MyLittleWidget.Views.Pages
{
  public sealed partial class DocklinePage : Page
  {
    private ConfigurationService _configService;
    public SharedViewModel ViewModel { get; } = SharedViewModel.Instance;

    // 垂直和水平辅助线
    private readonly List<double> _vGuideCoordinates = new List<double>();

    private readonly List<double> _hGuideCoordinates = new List<double>();
    private readonly List<Line> _vGuideLines = new List<Line>();
    private readonly List<Line> _hGuideLines = new List<Line>();

    public DocklinePage()
    {
      InitializeComponent();
      _configService = new ConfigurationService();
      this.Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
      EmbedIntoTargetWindow();
      LoadAndApplyConfiguration();
      var LineInfo = GetDesktop.GetDesktopGridInfo();
      var dpiScale = XamlRoot.RasterizationScale;
      SetupGuideLines(vLineCount: LineInfo.grid.X, vLineSpacing: LineInfo.spacing.X / ViewModel.Scale / dpiScale, hLineCount: LineInfo.grid.Y, hLineSpacing: LineInfo.spacing.Y / ViewModel.Scale / dpiScale);
      SharedViewModel.Instance.ConfigureGuides(_vGuideCoordinates, _hGuideCoordinates);
      ViewModel.WidgetList.CollectionChanged += OnWidgetsCollectionChanged;
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
      childWindow.AppWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
      var presenter = childWindow.AppWindow.Presenter as OverlappedPresenter;

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
      childWindow.Activate();
      PInvoke.SetParent(myHwnd, workerw);

    }
    // 添加和删除Widget
    private void OnWidgetsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
      {
        foreach (WidgetBase newItem in e.NewItems)
        {
          Debug.WriteLine(newItem.Content.GetType);
          RootCanvas.Children.Add(newItem);
          newItem.PositionUpdated += OnWidgetPositionUpdated;
        }
      }
      else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
      {
        foreach (WidgetBase oldItem in e.OldItems)
        {
          RootCanvas.Children.Remove(oldItem);
          oldItem.PositionUpdated -= OnWidgetPositionUpdated;
        }
      }
    }

    private void CreateDefaultWidgets()
    {
      var widgets = new ObservableCollection<WidgetBase>();
      ViewModel.ConfigureWidget(widgets);
    }

    private WidgetBase CreateWidgetFromType(WidgetConfig config)
    {
      
      if (config != null && !string.IsNullOrEmpty(config.WidgetType))
      {
        if (config.WidgetType == typeof(OneLineOfWisdom).FullName)
        {
          return new OneLineOfWisdom(config, AppSettings.Instance);
        }
        if (config.WidgetType == typeof(PomodoroClock).FullName)
        {
          return new PomodoroClock(config, AppSettings.Instance);
        }
      }

      return null;
    }

    //private WidgetBase CreateWidgetFromType(WidgetConfig loadedConfig)
    //{
    //  if (loadedConfig == null || string.IsNullOrEmpty(loadedConfig.WidgetType))
    //  {
    //    return null;
    //  }

    //  // 1. 从我们的注册服务中，根据名字查找类型
    //  Type widgetType = ComponentRegistryService.GetWidgetType(loadedConfig.WidgetType);

    //  if (widgetType == null)
    //  {
    //    // 在注册表中找不到这个类型，可能插件被删除了
    //    System.Diagnostics.Debug.WriteLine($"Widget type '{loadedConfig.WidgetType}' not found in registry.");
    //    return null;
    //  }

    //  try
    //  {
    //    // 2. 使用反射 (Activator.CreateInstance) 来创建实例
    //    //    这个方法会自动查找并调用匹配的构造函数。
    //    //    我们告诉它，我们要调用那个接收一个 WidgetConfig 的构造函数。
    //    object[] constructorArgs = { loadedConfig };
    //    var widgetInstance = Activator.CreateInstance(widgetType, constructorArgs);

    //    // 3. 将返回的 object 转换为我们需要的 WidgetBase 类型
    //    return widgetInstance as WidgetBase;
    //  }
    //  catch (Exception ex)
    //  {
    //    // 创建实例时可能失败（比如构造函数抛出异常）
    //    System.Diagnostics.Debug.WriteLine($"Failed to create instance of '{widgetType.FullName}': {ex.Message}");
    //    return null;
    //  }
    //}

    private void LoadAndApplyConfiguration()
    {
      var saveData = _configService.Load();
if (saveData != null)
      {
        AppSettings.Instance.BaseUnit = saveData.GlobalSettings.BaseUnit;
        AppSettings.Instance.IsDarkTheme = saveData.GlobalSettings.IsDarkTheme;

        var widgets = new ObservableCollection<WidgetBase>();
        if (saveData.WidgetConfigs != null && saveData.WidgetConfigs.Any())
        {
          foreach (var config_from_file in saveData.WidgetConfigs)
          {
            WidgetBase widget = CreateWidgetFromType(config_from_file);
            if (widget != null)
            {
              widgets.Add(widget);
            }
          }
        }
        ViewModel.ConfigureWidget(widgets);
      }
      else
      {
        CreateDefaultWidgets();
      }

      foreach (var widget in ViewModel.WidgetList)
      {
        RootCanvas.Children.Add(widget);
        widget.PositionUpdated += OnWidgetPositionUpdated;
      }
    }

    private void OnWidgetPositionUpdated(object sender, EventArgs e)
    {
      if (sender is WidgetBase widget && ViewModel.IsDragging && ViewModel.ActiveWidget == widget)
      {
        UpdateGuideVisibility(widget);
      }
    }

    private void ViewModel_PropertyChanged_ForGuideVisibility(object sender, PropertyChangedEventArgs e)
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

      double currentX = widget.Config.PositionX;
      double currentY = widget.Config.PositionY;
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