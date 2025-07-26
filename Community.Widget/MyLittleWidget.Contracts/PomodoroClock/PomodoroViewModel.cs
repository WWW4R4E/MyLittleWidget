using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;
using System.Windows.Input;

namespace PomodoroWidget
{
  public partial class PomodoroClockViewModel : ObservableObject
  {
    private DispatcherTimer _timer;

    [ObservableProperty]
    private TimeSpan _remainingTime = TimeSpan.Zero;

    [ObservableProperty]
    private string _timeDisplay = "00:00";

    [ObservableProperty]
    private bool _isTimerRunning = false;

    [ObservableProperty]
    private bool _isTimerStarted = false; // 标记是否已选择时间并开始计时

    // --- 命令 ---
    public ICommand SelectTimeCommand { get; }
    public ICommand StartStopCommand { get; }
    public ICommand ExitCommand { get; }

    public PomodoroClockViewModel()
    {
      SelectTimeCommand = new RelayCommand<TimeSpan>(ExecuteSelectTime);
      StartStopCommand = new RelayCommand(ExecuteStartStop);
      ExitCommand = new RelayCommand(ExecuteExit);

      _timer = new DispatcherTimer();
      _timer.Interval = TimeSpan.FromSeconds(1);
      _timer.Tick += Timer_Tick;
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

    private void Timer_Tick(object sender, object e)
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
      }
    }

    private void UpdateTimeDisplay()
    {
      TimeDisplay = $"{RemainingTime.Minutes:D2}:{RemainingTime.Seconds:D2}";
    }
  }
}