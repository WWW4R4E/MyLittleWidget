//namespace MyLittleWidget.CustomBase;

//public sealed partial class CustomControl1 : WidgetBase
//{
//  public CustomControl1(WidgetConfig config) : base(config)
//  {
//    // 创建你自己的内容(也可以使用xaml创建然后这里new出来)
//    var myContentGrid = new Grid
//    {
//      Background = new SolidColorBrush(Colors.AliceBlue)
//    };

//    var myText = new TextBlock
//    {
//      Text = "Hello, MyLittleWidget!",
//      FontSize = 24,
//      Foreground = new SolidColorBrush(Colors.Red),
//      HorizontalAlignment = HorizontalAlignment.Center,
//      VerticalAlignment = VerticalAlignment.Center
//    };

//    myContentGrid.Children.Add(myText);

//    // 获取基类已经创建好的 Content，它现在是一个 Border 对象
//    var baseBorder = this.Content as Border;
//    // 这将把你创建的 Grid 放入 Border 的内部。
//    baseBorder.Child = myContentGrid;
//  }
//  // 重写以配置组件相关属性
//  protected override void ConfigureWidget()
//  {
//    base.ConfigureWidget();
//    Config.Name = "CustomControl1";
//    Config.UnitWidth = 2;
//    Config.UnitHeight = 2;
//    Config.WidgetType = this.GetType().FullName;
//  }

//}