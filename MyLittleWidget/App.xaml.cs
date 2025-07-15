using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using MyLittleWidget.Services;
using MyLittleWidget.Utils;
using MyLittleWidget.Views;

namespace MyLittleWidget
{
  public partial class App : Application
  {
    public Window? window;
    public Window childWindow;

    public static DispatcherQueue DispatcherQueue { get; private set; }

    private const string AppKey = "c51f15b1-e8d6-4e1a-aca5-a0d63b14cc03";
    private AppInstance mainInstance;

    public App()
    {
      InitializeComponent();

      mainInstance = AppInstance.FindOrRegisterForKey(AppKey);
      if (!mainInstance.IsCurrent)
      {
        Task.Run(async () =>
        {
          var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
          await mainInstance.RedirectActivationToAsync(activatedArgs);
        }).GetAwaiter().GetResult();

        Environment.Exit(0);
      }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {

      DispatcherQueue = DispatcherQueue.GetForCurrentThread();
      ComponentRegistryService.DiscoverWidgets();
      mainInstance.Activated += OnAppActivated;

      childWindow = new ChildenWindow();

      childWindow.Closed += (sender, e) =>
      {
        // 当窗口关闭时，执行这个代码块
        // 创建一个 ConfigurationService 的临时实例，并调用 Save()
        // 这里我们不需要持有这个实例，用完即弃即可
        new ConfigurationService().Save();
      };

      if (GetShowMainWindow())
      {
        ShowMainWindow();
      }
    }

    private void OnAppActivated(object? sender, AppActivationArguments args)
    {
      DispatcherQueue.TryEnqueue(() =>
      {
        ShowMainWindow();
      });
    }

    private void ShowMainWindow()
    {
      window = new MainWindow();
      window.Activate();
      DisplayArea displayArea = DisplayArea.GetFromWindowId(window.AppWindow.Id, DisplayAreaFallback.Primary);

      int targetWidth = window.AppWindow.Size.Width - 300;
      int targetHeight = window.AppWindow.Size.Height;

      // 计算居中位置
      int centerX = displayArea.WorkArea.Width / 2 - targetWidth / 2 + displayArea.WorkArea.X;
      int centerY = displayArea.WorkArea.Height / 2 - targetHeight / 2 + displayArea.WorkArea.Y;

      RectInt32 rect = new RectInt32
      {
        X = centerX,
        Y = centerY,
        Width = targetWidth,
        Height = targetHeight
      };

      window.AppWindow.MoveAndResize(rect, displayArea);
    }

    private static bool GetShowMainWindow()
    {
      var settings = ApplicationData.Current.LocalSettings;
      if (settings.Values.TryGetValue("ShowMainWindow", out object? value) &&
          value is bool show)
      {
        return show;
      }
      return true;
    }
  }
}