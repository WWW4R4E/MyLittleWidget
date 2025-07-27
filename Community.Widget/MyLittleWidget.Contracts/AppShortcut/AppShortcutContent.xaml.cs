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
    // Ӧ��·������
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
          // ������ͳ����Ӧ��
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
          // ����UWPӦ�û�����Э��
          await Launcher.LaunchUriAsync(new Uri(ApplicationPath));
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"����Ӧ��ʧ��: {ex.Message}");
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
          Debug.WriteLine($"����ͼ��ʧ��: {ex.Message}");
          LoadDefaultIcon();
        }
    }

    // ����Ĭ��ͼ��
    private void LoadDefaultIcon()
    {
      // ����Ĭ��ͼ�꣬����ʹ��ϵͳĬ��Ӧ��ͼ��
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

    // �����ļ��Ϸ�
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
            UpdateDisplayName(); // ������ʾ����
          }
        }
      }
      e.Handled = true;
    }

    // �����Ϸ�
    private void AppShortcutContent_DragOver(object sender, DragEventArgs e)
    {
      e.AcceptedOperation = DataPackageOperation.Copy;
      e.Handled = true;
    }

    // ���ļ�·������ͼ��
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