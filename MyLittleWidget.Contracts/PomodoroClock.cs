using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using PomodoroWidget;
using System;

namespace MyLittleWidget.Contracts;
public sealed partial class PomodoroClock : WidgetBase
{
  private readonly PomodoroViewModel _viewModel = new PomodoroViewModel();

  public PomodoroClock(WidgetConfig config, IApplicationSettings settings) : base(config, settings)
  {
    // --- UI 构建 ---

    var mainGrid = new Grid
    {
      Padding = new Thickness(12),
      VerticalAlignment = VerticalAlignment.Center, 
      RowDefinitions =
    {
        new RowDefinition { Height = GridLength.Auto }, // 第0行：自适应
        new RowDefinition { Height = GridLength.Auto }, // 第1行：也改为自适应
        new RowDefinition { Height = GridLength.Auto }  // 第2行：自适应
    }
    };

    // 1. 状态文本
    var stateTextBlock = new TextBlock
    {
      FontSize = 16,
      FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
      HorizontalAlignment = HorizontalAlignment.Center
    };
    // 绑定状态
    stateTextBlock.SetBinding(TextBlock.TextProperty, new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroViewModel.CurrentState)),
      Mode = BindingMode.OneWay
    });
    Grid.SetRow(stateTextBlock, 0);

    // 2. 时间显示 (用Viewbox来自动缩放)
    var timeTextBlock = new TextBlock
    {
      FontFamily = new FontFamily("Segoe UI Variable Display"),
      FontWeight = Microsoft.UI.Text.FontWeights.Bold,
    };
    // 绑定时间显示
    timeTextBlock.SetBinding(TextBlock.TextProperty, new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroViewModel.TimeDisplay)),
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
      Path = new PropertyPath(nameof(PomodoroViewModel.StartStopCommand))
    });
    // 绑定内容文本 (需要一个Converter)
    var buttonContentBinding = new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroViewModel.IsTimerRunning)),
      Converter = new IsRunningToTextConverter() // 我们需要创建这个Converter
    };
    startStopButton.SetBinding(Button.ContentProperty, buttonContentBinding);

    var resetButton = new Button { Content = "Reset" };
    resetButton.SetBinding(Button.CommandProperty, new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroViewModel.ResetCommand))
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