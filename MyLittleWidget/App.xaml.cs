using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;
using MyLittleWidget.Services;
using MyLittleWidget.Utils;
using MyLittleWidget.Views;

namespace MyLittleWidget
{
  public partial class App : Application
  {
    public Window? window;
    public Window? childWindow;

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
      childWindow = new ChildenWindow(Properties.Settings.Default.IsPreview);
      SetupTrayIcon();
    }

    private void SetupTrayIcon()
    {
      // TODO 托盘实现

    }

    private void OnAppActivated(object? sender, AppActivationArguments args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
              WindowRegionUtil.RestoreWindowShape((HWND)WindowNative.GetWindowHandle(childWindow));

              ShowMainWindow();
            });
        }

    private void ShowMainWindow()
    {
      if (window == null)
      {
        window = new MainWindow();
        window.Closed += (sender, args) => { window = null; };
        window.Activate();

        DisplayArea displayArea = DisplayArea.GetFromWindowId(window.AppWindow.Id, DisplayAreaFallback.Primary);
        int targetWidth = window.AppWindow.Size.Width - 300;
        int targetHeight = window.AppWindow.Size.Height;
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
      else
      {
        window.Activate();
      }
    }

    private void ShowMainWindowFromTray()
    {
      DispatcherQueue.TryEnqueue(() =>
      {
        ShowMainWindow();
      });
    }
    private void ExitApplication()
    {
      Environment.Exit(0);
    }
  }
}