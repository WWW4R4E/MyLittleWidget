
using H.NotifyIcon;
using Microsoft.UI.Xaml.Input;
using MyLittleWidget.Views.Pages;

namespace MyLittleWidget.Views
{
  public sealed partial class ChildenWindow : Window
  {
    public ChildenWindow()
    {
      InitializeComponent();
      ExtendsContentIntoTitleBar = true;
    }

    private void ChildenWindow_OnClosed(object sender, WindowEventArgs args)
    {
      if (TrayIconView != null)
      {
        TrayIconView.Dispose();
      }
    }
    private void ShowWindowCommand_ExecuteRequested(object sender, ExecuteRequestedEventArgs args)
    {
      if (App.Current is App app)
      {
        app.LaunchMainWindow();
      }
    }

    private void ExitCommand_ExecuteRequested(object sender, ExecuteRequestedEventArgs args)
    {
      if (TrayIconView != null)
      {
        TrayIconView.Dispose();
      }
      App.Current.Exit(); 
    }
  }
}