using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MyLittleWidget.Contracts.AppShortcut
{
  public sealed partial class AppShortcutContent : UserControl
  {
    // 应用路径属性
    public string ApplicationPath { get; set; } = "";
    public string ApplicationArguments { get; set; } = "";
    public string IconPath { get; set; } = "";

    // 添加事件用于通知父组件
    public event EventHandler<AppShortcutEventArgs> SettingsRequested;
    public event EventHandler DeleteRequested;
    public event EventHandler EditRequested;

    public AppShortcutContent()
    {
      InitializeComponent();
      LoadDefaultIcon();
    }

    // 启动应用
    private async void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(ApplicationPath))
        return;

      try
      {
        if (ApplicationPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
          // 启动传统桌面应用
          var processStartInfo = new ProcessStartInfo
          {
            FileName = ApplicationPath,
            Arguments = ApplicationArguments,
            UseShellExecute = true
          };
          Process.Start(processStartInfo);
        }
        else
        {
          // 启动UWP应用或其他协议
          await Launcher.LaunchUriAsync(new Uri(ApplicationPath));
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"启动应用失败: {ex.Message}");
      }
    }

    // 设置菜单项点击
    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
      SettingsRequested?.Invoke(this, new AppShortcutEventArgs
      {
        ApplicationPath = ApplicationPath,
        Arguments = ApplicationArguments,
        IconPath = IconPath
      });
    }

    // 选择图标菜单项点击
    private async void SelectIconMenuItem_Click(object sender, RoutedEventArgs e)
    {
      // var picker = new FileOpenPicker();
      // var hwnd = WinRT.Interop.WindowNative.GetWindowHandle();
      // WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
      //
      // picker.ViewMode = PickerViewMode.Thumbnail;
      // picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
      // picker.FileTypeFilter.Add(".png");
      // picker.FileTypeFilter.Add(".jpg");
      // picker.FileTypeFilter.Add(".jpeg");
      // picker.FileTypeFilter.Add(".ico");
      // picker.FileTypeFilter.Add(".bmp");
      //
      // var file = await picker.PickSingleFileAsync();
      // if (file != null)
      // {
      //   IconPath = file.Path;
      //   SetAppIcon(IconPath);
      // }
    }

    // 编辑菜单项点击
    private void EditMenuItem_Click(object sender, RoutedEventArgs e)
    {
      EditRequested?.Invoke(this, EventArgs.Empty);
    }

    // 删除菜单项点击
    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
      DeleteRequested?.Invoke(this, EventArgs.Empty);
    }

    // 设置应用图标
    public void SetAppIcon(string iconPath)
    {
      if (!string.IsNullOrEmpty(iconPath) && System.IO.File.Exists(iconPath))
      {
        try
        {
          AppIconImage.Source = new BitmapImage(new Uri(iconPath));
          IconPath = iconPath;
        }
        catch
        {
          LoadDefaultIcon();
        }
      }
      else
      {
        LoadDefaultIcon();
      }
    }

    // 加载默认图标
    private void LoadDefaultIcon()
    {
      // 设置默认图标，或者使用系统默认应用图标
      AppIconImage.Source = new BitmapImage(
          new Uri("ms-appx:///Assets/AppIcon.png")); // 确保你有这个资源
    }

    // 设置应用路径
    public void SetApplicationPath(string path, string arguments = "")
    {
      ApplicationPath = path;
      ApplicationArguments = arguments;
    }
  }

  // 事件参数类
  public class AppShortcutEventArgs : EventArgs
  {
    public string ApplicationPath { get; set; }
    public string Arguments { get; set; }
    public string IconPath { get; set; }
  }
}