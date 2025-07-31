using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;

namespace MyLittleWidget.Contracts;

public sealed partial class OneLineOfWisdom : WidgetBase
{
  private TextBlock _quoteTextBlock;
  private readonly HttpClient _httpClient;

  public OneLineOfWisdom(WidgetConfig config, IApplicationSettings settings) : base(config, settings)
  {
    _httpClient = new HttpClient();
    var contentGrid = new Grid()
    {
      HorizontalAlignment = HorizontalAlignment.Stretch,
      VerticalAlignment = VerticalAlignment.Stretch,
    };

    contentGrid.Tapped += async (s, e) => await LoadQuoteAsync();
    _quoteTextBlock = new TextBlock
    {

      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      TextAlignment = TextAlignment.Center,
      TextWrapping = TextWrapping.Wrap,
      Margin = new Thickness(8) 
    };

    contentGrid.Children.Add(_quoteTextBlock);
    if (Content is Border baseBorder)
    {
      baseBorder.Child = contentGrid;
    }

    _ = LoadQuoteAsync();
  }

  private LinearGradientBrush CreateBackgroundBrush(bool isDarkTheme)
  {
    return new LinearGradientBrush
    {
      StartPoint = new Point(0, 0),
      EndPoint = new Point(1, 1),
      GradientStops = new GradientStopCollection
            {
                new GradientStop
                {
                    Color = isDarkTheme ?
                        Color.FromArgb(128, 40, 60, 100) :
                        Color.FromArgb(128, 60, 80, 120),
                    Offset = 0.0
                },
                new GradientStop
                {
                    Color = isDarkTheme ?
                        Color.FromArgb(128, 70, 100, 160) :
                        Color.FromArgb(128, 90, 120, 180),
                    Offset = 1.0
                }
            }
    };
  }

  protected override void ConfigureWidget()
  {
    base.ConfigureWidget();
    Config.Name = "每日一言";
    Config.UnitWidth = 6;
    Config.UnitHeight = 3;
    Config.WidgetType = GetType().FullName;
  }

  protected override void UpdateTheme(bool isDark)
  {
    if (Content is Border baseBordr)
    {
      baseBordr.Background = CreateBackgroundBrush(isDark);
    }
  }

  private async Task LoadQuoteAsync()
  {
    try
    {
      _quoteTextBlock.Text = "加载中...";

      var response = await _httpClient.GetAsync("https://api.nxvav.cn/api/yiyan/?encode=json&charset=utf-8");
      response.EnsureSuccessStatusCode();

      var json = await response.Content.ReadAsStringAsync();
      using var doc = JsonDocument.Parse(json);

      var quote = doc.RootElement.GetProperty("yiyan").GetString();
      var author = doc.RootElement.GetProperty("nick").GetString();

      SetQuoteTextWithFormatting(quote, author);

      AdjustFontSize(quote);
    }
    catch (Exception)
    {
      _quoteTextBlock.Text = "无法加载每日一言，请稍后重试";
      _quoteTextBlock.FontSize = 16; 
    }
  }

  private void SetQuoteTextWithFormatting(string quote, string author)
  {
    // 添加一言内容
    var quoteRun = new Run
    {
      Text = quote
    };

    // 添加换行和作者信息
    var authorRun = new Run
    {
      Text = $"\n\n— {author}",
      FontSize = 16,
      Foreground = new SolidColorBrush(AppSettings.IsDarkTheme ? Colors.LightBlue : Colors.Gray)
    };

    _quoteTextBlock.Inlines.Clear();
    _quoteTextBlock.Inlines.Add(quoteRun);
    _quoteTextBlock.Inlines.Add(authorRun);
  }

  private void AdjustFontSize(string quote)
  {
    int length = quote.Length;
    double fontSize;

    if (length <= 20)
    {
      fontSize = 22;
    }
    else if (length <= 40)
    {
      fontSize = 20;
    }
    else if (length <= 60)
    {
      fontSize = 18;
    }
    else
    {
      fontSize = 16;
    }

    if (_quoteTextBlock.Inlines.Count > 0 && _quoteTextBlock.Inlines[0] is Run quoteRun)
    {
      quoteRun.FontSize = fontSize;
    }
  }

  protected override void OnSettingsChanged()
  {
    base.OnSettingsChanged();
    UpdateTheme(AppSettings.IsDarkTheme);
  }

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    UpdateTheme(AppSettings.IsDarkTheme);
  }
}