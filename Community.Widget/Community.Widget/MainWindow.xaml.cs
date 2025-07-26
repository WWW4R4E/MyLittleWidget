using Microsoft.UI.Xaml;
using MyLittleWidget.Contracts;
using MyLittleWidget.Contracts.AppShortcut;
namespace Community.Widget
{
  public sealed partial class MainWindow : Window
  {
    public MainWindow()
    {
      ExtendsContentIntoTitleBar = true;
      InitializeComponent();
      Pad.Children.Add(new AppShortcut(new WidgetConfig(), new CommunityApplicationSettings()));
    }
  }
}
