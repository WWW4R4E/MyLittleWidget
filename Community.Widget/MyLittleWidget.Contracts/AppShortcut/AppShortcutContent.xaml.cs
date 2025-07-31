using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.System;

namespace MyLittleWidget.Contracts.AppShortcut
{

  public sealed partial class AppShortcutContent : UserControl
  {
    internal AppDataPaths AppData;
    internal IWidgetToolService WidgetTool;
    internal WidgetConfig WidgetConfig;

    public AppShortcutContent(WidgetConfig widgetConfig, IWidgetToolService widgetToolService)
    {
      InitializeComponent();
      WidgetConfig = widgetConfig;
      WidgetTool = widgetToolService;
      AppData = new AppDataPaths();
      UpdateContent();

    }


    public void UpdateContent()
    {
      if (WidgetConfig.CustomSettings.HasValue)
      {
        try
        {
          AppData = WidgetConfig.CustomSettings.Value.Deserialize<AppDataPaths>();
          AppIconImage.Source = new BitmapImage(new Uri(AppData.AppDisplayIcon));
        }
        catch (Exception ex)
        {
          Debug.WriteLine($"[AppShortcut] 反序列化异常: {ex.Message}");
        }
      }
    }


    private async void AppShortcutContent_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
      var pointerPoint = e.GetCurrentPoint(null); // null 表示相对于整个窗口

      if (pointerPoint.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
      {
        if (!string.IsNullOrEmpty(AppData.ApplicationPath))
        {
          await LaunchApplication();
        }
      }
    }



    private async Task LaunchApplication()
    {
      // TODO: 完善uwp应用启动
      try
      {
        if (AppData.ApplicationPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
          // 启动传统桌面应用
          var processStartInfo = new ProcessStartInfo
          {
            FileName = AppData.ApplicationPath,
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(AppData.ApplicationPath)
          };
          Process.Start(processStartInfo);
        }
        else
        {
          // 启动UWP应用或其他协议
          await Launcher.LaunchUriAsync(new Uri(AppData.ApplicationPath));
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"启动应用失败: {ex.Message}");
      }
    }

  }
}