using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;

namespace MyLittleWidget.Contracts;

public sealed partial class OneLineOfWisdom : WidgetBase
{
  private TextBlock _quoteTextBlock;
  private Button _refreshButton;

  public OneLineOfWisdom(WidgetConfig config, IApplicationSettings settings) : base(config, settings)
  {
    // 创建内容 Grid
    var contentGrid = new Grid();
    contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
    contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

    // 背景渐变层
    var backgroundBrush = new LinearGradientBrush
    {
      StartPoint = new Point(0, 0),
      EndPoint = new Point(1, 1),
      GradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Color.FromArgb(255, 60, 80, 120), Offset = 0.0 },
                new GradientStop { Color = Color.FromArgb(255, 90, 120, 180), Offset = 1.0 }
            }
    };

    // Border 设置渐变背景
    var border = new Border
    {
      Background = backgroundBrush,
      CornerRadius = new CornerRadius(12),
      Padding = new Thickness(10),
      Child = contentGrid
    };

    // 引言文本
    _quoteTextBlock = new TextBlock
    {
      Text = "加载中...",
      FontSize = 20,
      FontWeight = FontWeights.SemiBold,
      Foreground = new SolidColorBrush(Colors.White),
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      TextAlignment = TextAlignment.Center,
      TextWrapping = TextWrapping.Wrap,
      Margin = new Thickness(0, 8, 0, 0)
    };

    // 刷新按钮
    _refreshButton = new Button
    {
      Content = "🔄",
      FontSize = 16,
      Width = 36,
      Height = 36,
      HorizontalAlignment = HorizontalAlignment.Right,
      VerticalAlignment = VerticalAlignment.Top,
      Margin = new Thickness(0, 5, 5, 0),
      Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
      BorderBrush = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255)),
      Foreground = new SolidColorBrush(Colors.White),
      CornerRadius = new CornerRadius(18)
    };
    _refreshButton.Click += async (s, e) => await LoadQuoteAsync();

    // 布局
    contentGrid.Children.Add(_quoteTextBlock);
    contentGrid.Children.Add(_refreshButton);

    // 设置为 Border.Child
    var baseBorder = this.Content as Border;
    baseBorder.Child = border;

    // 初始化加载
    _ = LoadQuoteAsync();
  }

  protected override void ConfigureWidget()
  {
    base.ConfigureWidget();
    Config.Name = "每日一言";
    Config.UnitWidth = 2;
    Config.UnitHeight = 2;
    Config.WidgetType = this.GetType().FullName;
  }

  private async Task LoadQuoteAsync()
  {
    try
    {
      // 实际中你可以从网络加载
      var quote = "你所热爱的就是你的生活";
      _quoteTextBlock.Text = quote;
    }
    catch
    {
      _quoteTextBlock.Text = "无法加载每日一言";
    }
  }
}