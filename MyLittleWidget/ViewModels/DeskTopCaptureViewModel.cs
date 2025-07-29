using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Graphics.Canvas;
using MyLittleWidget.Contracts;
using MyLittleWidget.Utils;
using MyLittleWidget.Contracts.AppShortcut;
using MyLittleWidget.Services;

namespace MyLittleWidget.ViewModels
{
  internal partial class DeskTopCaptureViewModel : ObservableObject
  {
    // TODO 动态扫描加载小组件
    internal List<WidgetBase> Widgets = new() {
       new OneLineOfWisdom(new (),AppSettings.Instance),
       new PomodoroClock(new (),AppSettings.Instance, new WidgetToolService((nint)null)),
         new AppShortcut(new (),AppSettings.Instance,new WidgetToolService((nint)null)),
        };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewButtonText))]
    private bool _isPreviewing;

    [ObservableProperty]
    internal CanvasBitmap? _latestBitmap;

    // TODO: 完成和appsetting绑定
    [ObservableProperty] private int _selectedBackdropMaterial = 0;

    [ObservableProperty] private bool _isDarkTheme;

    [ObservableProperty] private double _baseUnit = 50;

    [ObservableProperty]
    private WidgetBase _selectedWidget;

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

    internal void ChildenWindow_VisibilityChanged(object sender, WindowVisibilityChangedEventArgs args)
    {
      if (args.Visible)
      {
        if (IsPreviewing)
        {
          _previewTimer.Start();
        }
      }
      else
      {
        _previewTimer.Stop();
      }
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
        LatestBitmap = null;
        CanvasDevice.GetSharedDevice().Trim();
        PreviewFrameReady?.Invoke();
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