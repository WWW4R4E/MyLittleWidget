
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MyLittleWidget.Contracts.AppShortcut;

public class AppShortcut : WidgetBase
{
  public AppShortcut(WidgetConfig config, IApplicationSettings settings) : base(config, settings)
  {

    if (Content is Border basBorder)
    {
      Content = new AppShortcutContent();
      Background = new SolidColorBrush(Colors.Transparent);
    }

  }
  protected override void ConfigureWidget()
  {
    base.ConfigureWidget();
    Config.Name = "快捷启动";
    Config.UnitWidth = 4;
    Config.UnitHeight = 4;
    Config.WidgetType = GetType().FullName;
  }
}