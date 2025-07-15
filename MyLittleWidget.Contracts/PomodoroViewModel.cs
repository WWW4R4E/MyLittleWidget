using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;
using System.Windows.Input;

namespace PomodoroWidget
{
  public enum PomodoroState
  {
    Stopped,
    Working,
    ShortBreak,
    LongBreak
  }

  public partial class PomodoroViewModel : ObservableObject
  {
    // --- 配置 ---
    private readonly TimeSpan _workDuration = TimeSpan.FromMinutes(25);
    private readonly TimeSpan _shortBreakDuration = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _longBreakDuration = TimeSpan.FromMinutes(15);
    private const int RoundsBeforeLongBreak = 4;

    // --- 内部状态 ---
    private DispatcherTimer _timer;
    private int _roundsCompleted = 0;

    // --- 可观察属性 (UI绑定) ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTimerRunning))]
    private PomodoroState _currentState = PomodoroState.Stopped;

    [ObservableProperty]
    private TimeSpan _remainingTime;

    [ObservableProperty]
    private string _timeDisplay = "25:00";

    public bool IsTimerRunning => CurrentState != PomodoroState.Stopped;

    // --- 命令 (UI绑定) ---
    public ICommand StartStopCommand { get; }
    public ICommand ResetCommand { get; }

    public PomodoroViewModel()
    {
      StartStopCommand = new RelayCommand(ExecuteStartStop);
      ResetCommand = new RelayCommand(ExecuteReset);

      _timer = new DispatcherTimer();
      _timer.Interval = TimeSpan.FromSeconds(1);
      _timer.Tick += Timer_Tick;

      // 初始化为工作状态
      ExecuteReset();
    }

    private void ExecuteStartStop()
    {
      if (IsTimerRunning)
      {
        // 暂停
        _timer.Stop();
        CurrentState = PomodoroState.Stopped;
      }
      else
      {
        // 开始或继续
        // 如果是第一次开始，重置到工作状态
        if (_remainingTime == TimeSpan.Zero)
        {
          CurrentState = PomodoroState.Working;
          RemainingTime = _workDuration;
        }
        _timer.Start();
        // 恢复状态，如果之前是停止了工作，就继续工作
        if (CurrentState == PomodoroState.Stopped)
        {
          // 这里需要一个逻辑来决定是恢复工作还是休息
          // 为简单起见，我们总是从工作开始
          CurrentState = PomodoroState.Working;
        }
      }
    }

    private void ExecuteReset()
    {
      _timer.Stop();
      _roundsCompleted = 0;
      CurrentState = PomodoroState.Stopped;
      RemainingTime = _workDuration;
      UpdateTimeDisplay();
    }

    private void Timer_Tick(object sender, object e)
    {
      RemainingTime = RemainingTime.Subtract(TimeSpan.FromSeconds(1));
      UpdateTimeDisplay();

      if (RemainingTime <= TimeSpan.Zero)
      {
        TransitionToNextState();
      }
    }

    private void TransitionToNextState()
    {
      _timer.Stop();

      if (CurrentState == PomodoroState.Working)
      {
        _roundsCompleted++;
        if (_roundsCompleted % RoundsBeforeLongBreak == 0)
        {
          CurrentState = PomodoroState.LongBreak;
          RemainingTime = _longBreakDuration;
        }
        else
        {
          CurrentState = PomodoroState.ShortBreak;
          RemainingTime = _shortBreakDuration;
        }
      }
      else // 从休息状态回到工作状态
      {
        CurrentState = PomodoroState.Working;
        RemainingTime = _workDuration;
      }

      UpdateTimeDisplay();
      // 可以选择自动开始下一轮
      _timer.Start();
    }

    private void UpdateTimeDisplay()
    {
      TimeDisplay = $"{RemainingTime.Minutes:D2}:{RemainingTime.Seconds:D2}";
    }
  }
}