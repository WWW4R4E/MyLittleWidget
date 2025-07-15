//namespace MyLittleWidget.CustomBase;

//public sealed partial class CustomControl2 : WidgetBase
//{
//  public CustomControl2(WidgetConfig config) : base(config)
//  {
//    DefaultStyleKey = typeof(CustomControl2);
//    Content = new TextBlock
//    {
//      Text = "Hello, Bro!",
//      FontSize = 24,
//      Foreground = new SolidColorBrush(Colors.White),
//    };
//  }

//  protected override void ConfigureWidget()
//  {
//    base.ConfigureWidget();
//    Config.Name = "CustomControl2";
//    Config.UnitWidth = 2;
//    Config.UnitHeight = 1;
//    Config.WidgetType = GetType().FullName;
//  }
//}