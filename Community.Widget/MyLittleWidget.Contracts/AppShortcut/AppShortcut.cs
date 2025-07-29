
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MyLittleWidget.Contracts.AppShortcut;

public class AppShortcut : WidgetBase
{
  public AppShortcut(WidgetConfig config, IApplicationSettings settings, IWidgetToolService widgetTool) : base(config, settings, widgetTool)
  {
    var appShortcutContent = new AppShortcutContent(config, widgetTool);
    if (Content is Border basBorder)
    {
      basBorder.Child = appShortcutContent;
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