using MyLittleWidget.Views.Pages;

namespace MyLittleWidget.Views
{
  public sealed partial class ChildenWindow : Window
  {
    public ChildenWindow()
    {
      InitializeComponent();
      ExtendsContentIntoTitleBar = true;
      Content = new DocklinePage();
    }
  }
}