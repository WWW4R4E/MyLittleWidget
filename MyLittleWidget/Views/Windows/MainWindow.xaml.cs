using Microsoft.Windows.AppLifecycle;

namespace MyLittleWidget
{
  public sealed partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      ExtendsContentIntoTitleBar = true;
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
      args.Handled = true;
      var currentSize = AppWindow.Size;
      Properties.Settings.Default.WindowSize = new System.Drawing.Size(currentSize.Width, currentSize.Height);
      Properties.Settings.Default.Save();
      AppInstance.Restart(string.Empty);
    }
  }
}