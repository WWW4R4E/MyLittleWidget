namespace MyLittleWidget.CustomBase;

public sealed partial class CustomControl2 : WidgetBase
{
    public CustomControl2()
    {
        this.Config = new WidgetConfig
        {
            Id = 2,
            Name = "CustomControl2",
            UnitWidth = 2,
            UnitHeight = 1
        };
        DefaultStyleKey = typeof(CustomControl2);
        Content = new TextBlock
        {
            Text = "Hello, Bro!",
            FontSize = 24,
            Foreground = new SolidColorBrush(Colors.White),
        };
    }
}
