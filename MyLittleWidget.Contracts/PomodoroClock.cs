using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using PomodoroWidget;
using System;
using Windows.Foundation;

namespace MyLittleWidget.Contracts;
public sealed partial class PomodoroClock : WidgetBase
{
  private readonly PomodoroColor _viewModel = new PomodoroColor();

  public PomodoroClock(WidgetConfig config, IApplicationSettings settings) : base(config, settings)
  {

    var mainGrid = new Grid
    {
      CornerRadius = new CornerRadius(12),
      Padding = new Thickness(12),
      

      Background = new LinearGradientBrush
      {
        StartPoint = new Point(0, 0),
        EndPoint = new Point(1, 1),
        GradientStops = new GradientStopCollection
        {
          new GradientStop { Color = Colors.DeepSkyBlue, Offset = 0.0 },
          new GradientStop { Color = Colors.MediumPurple, Offset = 1.0 }
        }
      },

      RowDefinitions =
      {
        new RowDefinition { Height = GridLength.Auto },
        new RowDefinition { Height = GridLength.Auto },
        new RowDefinition { Height = GridLength.Auto }
      }
    };

    var stateTextBlock = new TextBlock
    {
      FontSize = 16,
      FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
      HorizontalAlignment = HorizontalAlignment.Center
    };
    stateTextBlock.SetBinding(TextBlock.TextProperty, new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroColor.CurrentState)),
      Mode = BindingMode.OneWay
    });
    Grid.SetRow(stateTextBlock, 0);

    var timeTextBlock = new TextBlock
    {
      FontFamily = new FontFamily("Segoe UI Variable Display"),
      FontWeight = Microsoft.UI.Text.FontWeights.Bold,
    };
    timeTextBlock.SetBinding(TextBlock.TextProperty, new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroColor.TimeDisplay)),
      Mode = BindingMode.OneWay,
      FallbackValue = "25:00"
    });
    var viewbox = new Viewbox
    {
      Child = timeTextBlock,
      Margin = new Thickness(0, 10, 0, 10)
    };
    Grid.SetRow(viewbox, 1);

    // 3. 按钮面板
    var startStopButton = new Button();
    // 绑定命令
    startStopButton.SetBinding(Button.CommandProperty, new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroColor.StartStopCommand))
    });
    // 绑定内容文本 (需要一个Converter)
    var buttonContentBinding = new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroColor.IsTimerRunning)),
      Converter = new IsRunningToTextConverter() // 我们需要创建这个Converter
    };
    startStopButton.SetBinding(Button.ContentProperty, buttonContentBinding);

    var resetButton = new Button { Content = "Reset" };
    resetButton.SetBinding(Button.CommandProperty, new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroColor.ResetCommand))
    });

    var buttonPanel = new StackPanel
    {
      Orientation = Orientation.Horizontal,
      HorizontalAlignment = HorizontalAlignment.Center,
      Spacing = 8,
      Children = { startStopButton, resetButton }
    };
    Grid.SetRow(buttonPanel, 2);

    // 将所有UI元素添加到Grid中
    mainGrid.Children.Add(stateTextBlock);
    mainGrid.Children.Add(viewbox);
    mainGrid.Children.Add(buttonPanel);

    // 将构建好的Grid设置为我们控件的内容
    this.Content = mainGrid;
  }

  // 这个方法由基类的构造函数调用，用来设置组件的元数据
  protected override void ConfigureWidget()
  {
    base.ConfigureWidget();
    Config.Name = "Pomodoro Clock";
    Config.UnitWidth = 2; // 2x2的方形组件
    Config.UnitHeight = 2;
    Config.WidgetType = this.GetType().FullName;
  }
}

// --- 辅助的Converter ---
public class IsRunningToTextConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language)
  {
    if (value is bool isRunning)
    {
      return isRunning ? "Pause" : "Start";
    }
    return "Start";
  }

  public object ConvertBack(object value, Type targetType, object parameter, string language)
  {
    throw new NotImplementedException();
  }
}