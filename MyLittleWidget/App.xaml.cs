using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Input;
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

    }

    private void LaunchWidgetWindow()
    {
        WidgetWindow = new ChildenWindow();
    }
    internal void LaunchMainWindow()
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

    
    }
  }
