using MyLittleWidget.Custom;

namespace MyLittleWidget.CustomBase
{
    public class TestWidget : WidgetBase
    {
        public TestWidget()
        {
            if (this.Content is Border contentBorder)
            {
                var textBlock = new TextBlock
                {
                    Text = "I am a Widget!",
                    FontSize = 20,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                contentBorder.Child = textBlock;
                contentBorder.Background = new SolidColorBrush(Colors.Gold);
            }
        }
        
    }
}
