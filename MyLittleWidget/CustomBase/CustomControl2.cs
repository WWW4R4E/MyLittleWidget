namespace MyLittleWidget.CustomBase;

public sealed partial class CustomControl2 : WidgetBase
{
    public CustomControl2()
    {
        DefaultStyleKey = typeof(CustomControl2);
        Content = new TextBlock
        {
            Text = "Hello, Bro!",
            FontSize = 24,
            Foreground = new SolidColorBrush(Colors.White),
        };
    }
}
