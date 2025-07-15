using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MyLittleWidget.Contracts;

public sealed partial class CustomControl2 : WidgetBase
{
  public CustomControl2(WidgetConfig config, IApplicationSettings settings) : base(config, settings)
  {
    DefaultStyleKey = typeof(CustomControl2);
    Content = new TextBlock
    {
      Text = "Hello, Bro!",
      FontSize = 24,
      Foreground = new SolidColorBrush(Colors.White),
    };
  }

  protected override void ConfigureWidget()
  {
    base.ConfigureWidget();
    Config.Name = "CustomControl2";
    Config.UnitWidth = 2;
    Config.UnitHeight = 1;
    Config.WidgetType = GetType().FullName;
  }
}