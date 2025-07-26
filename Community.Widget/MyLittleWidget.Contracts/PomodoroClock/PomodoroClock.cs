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
  private readonly PomodoroClockViewModel _viewModel = new ();

  public PomodoroClock(WidgetConfig config, IApplicationSettings settings) : base(config, settings)
  {
    var contentGrid = new Grid
    {
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
      Padding = new Thickness(0,24,0,24)
    };

    // 定义行
    contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); 
    contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); 
    contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); 

    // 选择时间时的标题
    var selectTitleTextBlock = new TextBlock
    {
      Text = "选择专注时间",
      FontSize = 16,
      HorizontalAlignment = HorizontalAlignment.Center,
      FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
      Margin = new Thickness(0, 0, 0, 20)
    };
    Grid.SetRow(selectTitleTextBlock, 0);

    // 计时时的标题（默认隐藏）
    var timerTitleTextBlock = new TextBlock
    {
      Text = "专注时间",
      FontSize = 16,
      HorizontalAlignment = HorizontalAlignment.Center,
      FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
      Visibility = Visibility.Collapsed
    };
    Grid.SetRow(timerTitleTextBlock, 0);

    var contentPanel = new Grid();
    Grid.SetRow(contentPanel, 1);

    var presetButtonsPanel = new Grid();
    presetButtonsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
    presetButtonsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
    presetButtonsPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

    // 10分钟按钮（左上）
    var button10 = CreatePresetButton(10, 55);
    Grid.SetRow(button10, 0);
    Grid.SetColumn(button10, 0);
    presetButtonsPanel.Children.Add(button10);

    // 15分钟按钮（右上）
    var button15 = CreatePresetButton(15, 55);
    Grid.SetRow(button15, 0);
    Grid.SetColumn(button15, 1);
    presetButtonsPanel.Children.Add(button15);

    // 30分钟按钮（下排）
    var button30 = CreatePresetButton(30, 65);
    Grid.SetRow(button30, 1);
    Grid.SetRowSpan(button30,2);
    Grid.SetColumn(button30, 0);
    Grid.SetColumnSpan(button30, 2);
    presetButtonsPanel.Children.Add(button30);

    var timeDisplayPanel = new StackPanel
    {
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      Visibility = Visibility.Collapsed
    };

    var timeTextBlock = new TextBlock
    {
      FontFamily = new FontFamily("Segoe UI Variable Display"),
      FontWeight = Microsoft.UI.Text.FontWeights.Bold,
      HorizontalAlignment = HorizontalAlignment.Center,
      FontSize = 48,
      Foreground = new SolidColorBrush(Colors.White)
    };
    timeTextBlock.SetBinding(TextBlock.TextProperty, new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroClockViewModel.TimeDisplay)),
      Mode = BindingMode.OneWay,
    });

    timeDisplayPanel.Children.Add(timeTextBlock);

    contentPanel.Children.Add(presetButtonsPanel);
    contentPanel.Children.Add(timeDisplayPanel);

    var controlButtonsPanel = new StackPanel
    {
      Orientation = Orientation.Horizontal,
      HorizontalAlignment = HorizontalAlignment.Center,
      Spacing = 4,
    };
    Grid.SetRow(controlButtonsPanel, 2);

    var startStopButton = new Button
    {
      Content = "开始",
      MinWidth = 80,
      Visibility = Visibility.Collapsed
    };
    startStopButton.SetBinding(Button.CommandProperty, new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroClockViewModel.StartStopCommand))
    });
    startStopButton.SetBinding(Button.ContentProperty, new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroClockViewModel.IsTimerRunning)),
      Converter = new IsRunningToTextConverter()
    });

    var exitButton = new Button
    {
      Content = "退出",
      MinWidth = 80,
      Visibility = Visibility.Collapsed
    };
    exitButton.SetBinding(Button.CommandProperty, new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroClockViewModel.ExitCommand))
    });

    controlButtonsPanel.Children.Add(startStopButton);
    controlButtonsPanel.Children.Add(exitButton);


    var selectTitleVisibilityBinding = new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroClockViewModel.IsTimerStarted)),
      Converter = new InverseBooleanToVisibilityConverter()
    };
    selectTitleTextBlock.SetBinding(VisibilityProperty, selectTitleVisibilityBinding);

    // 计时时的标题可见性
    var timerTitleVisibilityBinding = new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroClockViewModel.IsTimerStarted)),
      Converter = new BooleanToVisibilityConverter()
    };
    timerTitleTextBlock.SetBinding(VisibilityProperty, timerTitleVisibilityBinding);

    var presetPanelVisibilityBinding = new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroClockViewModel.IsTimerStarted)),
      Converter = new InverseBooleanToVisibilityConverter()
    };
    presetButtonsPanel.SetBinding(VisibilityProperty, presetPanelVisibilityBinding);

    var timePanelVisibilityBinding = new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroClockViewModel.IsTimerStarted)),
      Converter = new BooleanToVisibilityConverter()
    };
    timeDisplayPanel.SetBinding(VisibilityProperty, timePanelVisibilityBinding);

    var controlButtonsVisibilityBinding = new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroClockViewModel.IsTimerStarted)),
      Converter = new BooleanToVisibilityConverter()
    };
    startStopButton.SetBinding(VisibilityProperty, controlButtonsVisibilityBinding);
    exitButton.SetBinding(VisibilityProperty, controlButtonsVisibilityBinding);

    contentGrid.Children.Add(selectTitleTextBlock);
    contentGrid.Children.Add(timerTitleTextBlock);
    contentGrid.Children.Add(contentPanel);
    contentGrid.Children.Add(controlButtonsPanel);

    if (Content is Border baseBord)
    {
      baseBord.Child = contentGrid;
    }
  }

  private Button CreatePresetButton(int minutes, int size)
  {
    var button = new Button
    {
      Width = size,
      Height = size,
      CornerRadius = new CornerRadius(size),
      FontWeight = Microsoft.UI.Text.FontWeights.Bold,
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center
    };

    var contentPanel = new StackPanel
    {
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
    };

    var timeTextBlock = new TextBlock
    {
      Text = minutes.ToString(),
      FontSize =14,
      HorizontalAlignment = HorizontalAlignment.Center,
      FontWeight = Microsoft.UI.Text.FontWeights.Bold
    };


    contentPanel.Children.Add(timeTextBlock);

    button.Content = contentPanel;

    var commandBinding = new Binding
    {
      Source = _viewModel,
      Path = new PropertyPath(nameof(PomodoroClockViewModel.SelectTimeCommand))
    };
    button.SetBinding(Button.CommandProperty, commandBinding);

    button.CommandParameter = TimeSpan.FromMinutes(minutes);

    return button;
  }

  protected override void ConfigureWidget()
  {
    base.ConfigureWidget();
    Config.Name = "番茄钟";
    Config.UnitWidth = 4;
    Config.UnitHeight = 4;
    Config.WidgetType = GetType().FullName;
  }
}

public class IsRunningToTextConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language)
  {
    if (value is bool isRunning)
    {
      return isRunning ? "暂停" : "开始";
    }
    return "开始";
  }

  public object ConvertBack(object value, Type targetType, object parameter, string language)
  {
    throw new NotImplementedException();
  }
}

public class InverseBooleanToVisibilityConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language)
  {
    if (value is bool boolValue)
    {
      return boolValue ? Visibility.Collapsed : Visibility.Visible;
    }
    return Visibility.Visible;
  }

  public object ConvertBack(object value, Type targetType, object parameter, string language)
  {
    throw new NotImplementedException();
  }
}

public class BooleanToVisibilityConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language)
  {
    if (value is bool boolValue)
    {
      return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }
    return Visibility.Collapsed;
  }

  public object ConvertBack(object value, Type targetType, object parameter, string language)
  {
    throw new NotImplementedException();
  }
}