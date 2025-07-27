using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using MyLittleWidget.Contracts;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PomodoroWidget
{
  public partial class PomodoroClockViewModel : ObservableObject
  {
    private readonly IWidgetToolService _toolService;
    private DispatcherTimer _timer;

    [ObservableProperty]
    private TimeSpan _remainingTime = TimeSpan.Zero;

    [ObservableProperty]
    private string _timeDisplay = "00:00";

    [ObservableProperty]
    private bool _isTimerRunning = false;

    [ObservableProperty]
    private bool _isTimerStarted = false; 

    // --- 命令 ---
    public ICommand SelectTimeCommand { get; }
    public ICommand StartStopCommand { get; }
    public ICommand ExitCommand { get; }

    public PomodoroClockViewModel(IWidgetToolService toolService)
    {
      _toolService = toolService;
      SelectTimeCommand = new RelayCommand<TimeSpan>(ExecuteSelectTime);
      StartStopCommand = new RelayCommand(ExecuteStartStop);
      ExitCommand = new RelayCommand(ExecuteExit);

      _timer = new DispatcherTimer();
      _timer.Interval = TimeSpan.FromSeconds(1);
      _timer.Tick += Timer_TickAsync;

    }

    private void ExecuteSelectTime(TimeSpan selectedTime)
    {
      RemainingTime = selectedTime;
      IsTimerStarted = true;
      UpdateTimeDisplay();
    }

    private void ExecuteStartStop()
    {
      if (IsTimerRunning)
      {
        _timer.Stop();
        IsTimerRunning = false;
      }
      else
      {
        _timer.Start();
        IsTimerRunning = true;
      }
      UpdateTimeDisplay();
    }

    private void ExecuteExit()
    {
      _timer.Stop();
      IsTimerRunning = false;
      IsTimerStarted = false;
      RemainingTime = TimeSpan.Zero;
      UpdateTimeDisplay();
    }
    private void Timer_TickAsync(object sender, object e)
    {
      if (RemainingTime > TimeSpan.Zero)
      {
        RemainingTime = RemainingTime.Subtract(TimeSpan.FromSeconds(1));
        UpdateTimeDisplay();
      }
      else
      {
        _timer.Stop();
        IsTimerRunning = false;
        _= _toolService.ShowNotificationAsync("计时完成", "您的专注时间已结束！");
      }
    }

    private void UpdateTimeDisplay()
    {
      TimeDisplay = $"{RemainingTime.Minutes:D2}:{RemainingTime.Seconds:D2}";
    }
  }
}