using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Input;
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