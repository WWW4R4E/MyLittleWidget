using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Windows.Storage;
using Windows.System;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Input;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;
using System.IO;

namespace MyLittleWidget.Contracts.AppShortcut
{
  public sealed partial class AppShortcutContent : UserControl
  {
    // 应用路径属性
    public string ApplicationPath { get; set; } = "";
    public string ApplicationArguments { get; set; } = "";
    private string AppDisplayName { get; set; } = ""; 

    public AppShortcutContent()
    {
      InitializeComponent();
      LoadDefaultIcon();
    }

    private async void AppShortcutContent_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(ApplicationPath))
        return;

      await LaunchApplication();
    }


    private async Task LaunchApplication()
    {
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
    public void SetAppIcon(BitmapImage icon)
    {

        try
        {
          AppIconImage.Source = icon;

        }
        catch (Exception ex)
        {
          Debug.WriteLine($"加载图标失败: {ex.Message}");
          LoadDefaultIcon();
        }
    }

    // 加载默认图标
    private void LoadDefaultIcon()
    {
      // 设置默认图标，或者使用系统默认应用图标
      AppIconImage.Source = new BitmapImage(
          new Uri("ms-appx:///Assets/StoreLogo.png")); 
    }
    private void UpdateDisplayName()
    {

        if (!string.IsNullOrEmpty(ApplicationPath))
        {
          AppDisplayName = Path.GetFileNameWithoutExtension(ApplicationPath);
          AppDisplayNameTextBlock.Text = AppDisplayName; 
        }
        else
        {
          AppDisplayName = "";
          AppDisplayNameTextBlock.Text = "";
        }
    }

    // 处理文件拖放
    private async void AppShortcutContent_Drop(object sender, DragEventArgs e)
    {
      if (e.DataView.Contains(StandardDataFormats.StorageItems))
      {
        var items = await e.DataView.GetStorageItemsAsync();
        if (items.Count > 0)
        {
          if (items[0] is StorageFile file)
          {
            ApplicationPath = file.Path;
            SetAppIconFromPath(ApplicationPath);
            UpdateDisplayName(); // 更新显示名称
          }
        }
      }
      e.Handled = true;
    }

    // 允许拖放
    private void AppShortcutContent_DragOver(object sender, DragEventArgs e)
    {
      e.AcceptedOperation = DataPackageOperation.Copy;
      e.Handled = true;
    }

    // 从文件路径设置图标
    private void SetAppIconFromPath(string filePath)
    {
        if (filePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
BitmapImage Icon = ExtractIconFromExe(filePath); 
          if (Icon != null)
          {
            SetAppIcon(Icon);
          }
          else
          {
            LoadDefaultIcon();
          }
        }
    }

    private BitmapImage ExtractIconFromExe(string exePath)
    {
        using (System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath))
        {
          if (icon != null)
          {
            using (var bmp = icon.ToBitmap())
            {
              using (var ms = new MemoryStream())
              {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(ms.AsRandomAccessStream());
                return bitmapImage;
              }
            }
          }
        }
      return null;
    }
  }
}