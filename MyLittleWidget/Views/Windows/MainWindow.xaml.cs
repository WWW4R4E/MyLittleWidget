// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyLittleWidget
{
  /// <summary>
  /// An empty window that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      ExtendsContentIntoTitleBar = true;
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
      //((App)Application.Current).window = null;
      Close();
    }
  }
}