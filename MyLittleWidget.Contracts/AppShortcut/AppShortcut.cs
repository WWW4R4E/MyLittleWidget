
using Microsoft.UI.Xaml.Controls;

namespace MyLittleWidget.Contracts.AppShortcut;

public class AppShortcut : WidgetBase
{
  public AppShortcut(WidgetConfig config, IApplicationSettings settings) : base(config, settings)
  {
    if (Content is Border basBorder)
    {
      basBorder.Child = new AppShortcutContent();
    }

  }
}