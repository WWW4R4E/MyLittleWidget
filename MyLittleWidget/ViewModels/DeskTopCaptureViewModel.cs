using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Graphics.Canvas;
using MyLittleWidget.Contracts;
using MyLittleWidget.Models;
using MyLittleWidget.Utils;

namespace MyLittleWidget.ViewModels
{
  internal partial class DeskTopCaptureViewModel : ObservableObject
  {
    internal List<LittleWidget> littleWidgets = new() {
        new() { Title = "小组件1",widget = new OneLineOfWisdom(new WidgetConfig(),AppSettings.Instance)},
        new() { Title = "小组件2",widget = new PomodoroClock(new WidgetConfig(),AppSettings.Instance)},
        };
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewButtonText))]
    private bool _isPreviewing;
    [ObservableProperty]
    internal CanvasBitmap? _latestBitmap;
    public string PreviewButtonText => IsPreviewing ? "停止预览" : "开始预览";
    private readonly DispatcherTimer _previewTimer;
    public event Action? PreviewFrameReady;

    internal DispatcherTimer timer = new();
    internal float scale;
    internal float Dpiscale = GetDesktop.GetSystemDpiScale();
    public DeskTopCaptureViewModel()
    {
      _previewTimer = new DispatcherTimer
      {
        Interval = TimeSpan.FromMilliseconds(30)
      };
      _previewTimer.Tick += async (s, e) => await RefreshCaptureAsync();
    }
    partial void OnIsPreviewingChanged(bool value)
    {
      if (value)
      {
        _previewTimer.Start();
      }
      else
      {
        _previewTimer.Stop();
        LatestBitmap?.Dispose();
        LatestBitmap = null; // 设置为null，通知UI清除
        CanvasDevice.GetSharedDevice().Trim();
        PreviewFrameReady?.Invoke(); // 触发一次刷新来清除画布
      }
    }
    [RelayCommand]
    private void TogglePreview()
    {
      IsPreviewing = !IsPreviewing;
    }
    private async Task RefreshCaptureAsync()
    {
      using var softwareBitmap = GetDesktop.CaptureWindow();
      if (softwareBitmap != null)
      {
        LatestBitmap?.Dispose();
        LatestBitmap = CanvasBitmap.CreateFromSoftwareBitmap(CanvasDevice.GetSharedDevice(), softwareBitmap);
        PreviewFrameReady?.Invoke();
      }
    }

  }
}