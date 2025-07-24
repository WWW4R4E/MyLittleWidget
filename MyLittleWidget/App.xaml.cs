using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;
using MyLittleWidget.Services;
using MyLittleWidget.Views;

namespace MyLittleWidget
{
  public partial class App : Application
  {
    public Window? MainWindow;
    public Window? WidgetWindow;

    public static DispatcherQueue DispatcherQueue { get; private set; }

    private const string AppKey = "c51f15b1-e8d6-4e1a-aca5-a0d63b14cc03";
    private AppInstance mainInstance;


    public App()
    {
      InitializeComponent();

      var mainInstance = AppInstance.FindOrRegisterForKey(AppKey);
      if (!mainInstance.IsCurrent)
      {
        var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
        Task.Run(async () => await mainInstance.RedirectActivationToAsync(activatedArgs)).GetAwaiter().GetResult();
        Environment.Exit(0); 
      }
      else
      {
        mainInstance.Activated += OnAppActivated;
      }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
      DispatcherQueue = DispatcherQueue.GetForCurrentThread();
      ComponentRegistryService.DiscoverWidgets();

      if (!Properties.Settings.Default.IsPreview)
      {
        LaunchWidgetWindow();
      }
      else
      {
        LaunchMainWindow();
      }

      SetupTrayIcon();
    }
    private void SetupTrayIcon()
    {
      // TODO 托盘实现

    }
    private void LaunchWidgetWindow()
    {
        WidgetWindow = new ChildenWindow();
    }
    private void LaunchMainWindow()
    {
      Properties.Settings.Default.IsPreview = true;
      Properties.Settings.Default.Save();
      if (MainWindow == null)
      {
        MainWindow = new MainWindow();
        MainWindow.Activate();
      }
      else
      {
        MainWindow.Activate();
      }
      WidgetWindow?.Close();
      WidgetWindow = new ChildenWindow();
    }
    private void OnAppActivated(object? sender, AppActivationArguments args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
              LaunchMainWindow();
            });
        }

    // private void ShowMainWindow()
    // {
    //   if (MainWindow == null)
    //   {
    //     MainWindow = new MainWindow();
    //     MainWindow.Closed += (sender, args) => { MainWindow = null; };
    //     MainWindow.Activate();
    //
    //     DisplayArea displayArea = DisplayArea.GetFromWindowId(MainWindow.AppWindow.Id, DisplayAreaFallback.Primary);
    //     int targetWidth = MainWindow.AppWindow.Size.Width - 300;
    //     int targetHeight = MainWindow.AppWindow.Size.Height;
    //     int centerX = displayArea.WorkArea.Width / 2 - targetWidth / 2 + displayArea.WorkArea.X;
    //     int centerY = displayArea.WorkArea.Height / 2 - targetHeight / 2 + displayArea.WorkArea.Y;
    //     RectInt32 rect = new RectInt32
    //     {
    //       X = centerX,
    //       Y = centerY,
    //       Width = targetWidth,
    //       Height = targetHeight
    //     };
    //     MainWindow.AppWindow.MoveAndResize(rect, displayArea);
    //   }
    //   else
    //   {
    //     MainWindow.Activate();
    //   }
    // }
  }
}