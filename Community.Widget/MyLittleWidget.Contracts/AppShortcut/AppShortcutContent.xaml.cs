using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Shell;

namespace MyLittleWidget.Contracts.AppShortcut
{

  public sealed partial class AppShortcutContent : UserControl
  {
    // 应用路径数据类
    internal class AppDataPaths
    {
      public string ApplicationPath { get; set; } = string.Empty;
      public string ApplicationArguments { get; set; } = string.Empty;
      public string AppDisplayIcon { get; set; } = "ms-appx:///Assets/AppIcon.scale-400.png"; // 添加默认图标
    }
    internal AppDataPaths AppData;
    internal IWidgetToolService WidgetTool;
    internal WidgetConfig WidgetConfig;

    public AppShortcutContent(WidgetConfig widgetConfig, IWidgetToolService widgetToolService)
    {
      InitializeComponent();
      WidgetConfig = widgetConfig;
      WidgetTool = widgetToolService;

      Debug.WriteLine($"[AppShortcut] 构造函数: CustomSettings.HasValue = {widgetConfig.CustomSettings.HasValue}");

      // 强制初始化AppData
      AppData = new AppDataPaths();

      if (widgetConfig.CustomSettings.HasValue)
      {
        try
        {
          var deserialized = widgetConfig.CustomSettings.Value.Deserialize<AppDataPaths>();
          if (deserialized != null)
          {
            // 显式处理空值情况
            AppData.ApplicationPath = deserialized.ApplicationPath ?? string.Empty;
            AppData.ApplicationArguments = deserialized.ApplicationArguments ?? string.Empty;
            AppData.AppDisplayIcon = deserialized.AppDisplayIcon ?? "ms-appx:///Assets/DefaultIcon.png";
          }
        }
        catch (Exception ex)
        {
          Debug.WriteLine($"[AppShortcut] 反序列化异常: {ex.Message}");
          // 使用已初始化的默认值
        }
      }

    }


    private async void AppShortcutContent_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(AppData.ApplicationPath))
        return;
      await LaunchApplication();
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
    public void SetAppIcon(string icon)
    {
      try
      {
        AppIconImage.Source = new BitmapImage(new Uri(icon));

      }
      catch (Exception ex)
      {
        Debug.WriteLine($"设置图标失败: {ex.Message}");
      }
    }

    // 保存AppData到WidgetConfig
    private void SaveAppData()
    {
      Debug.WriteLine($"[AppShortcut] 保存数据: ApplicationPath={AppData.ApplicationPath}");
      WidgetConfig.CustomSettings = JsonSerializer.SerializeToElement(AppData);
      Debug.WriteLine($"[AppShortcut] 序列化后的CustomSettings: {WidgetConfig.CustomSettings}");
      WidgetTool.SaveWidegtDataAsync();
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
            AppData.ApplicationPath = file.Path;
            SetAppIconFromPath(AppData.ApplicationPath);
          }
        }
      }
      e.Handled = true;
    }

    // 处理拖放
    private void AppShortcutContent_DragOver(object sender, DragEventArgs e)
    {
      e.AcceptedOperation = DataPackageOperation.Copy;
      e.Handled = true;
    }

    private void SetAppIconFromPath(string filePath)
    {
      if (filePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
      {
        var icoPath = SaveIconFromExe(filePath);
        if (icoPath != null)
        {
          SetAppIcon(icoPath);
          AppData.AppDisplayIcon = icoPath;
          // UpdateDisplayName() 会调用 SaveAppData()
        }
      }
    }

    // TODO 将com部分移入公共区域

    public static readonly Guid IID_IShellItem = new("43826d1e-e718-42ee-bc55-a1e261c37bfe");
    public static readonly Guid IID_IShellItemImageFactory = new("bcc18b79-ba16-442f-80c4-8a59c30c463b");

    private unsafe string SaveIconFromExe(string exePath)
    {

      IShellItem* pShellItem = null;
      HRESULT hr = PInvoke.SHCreateItemFromParsingName(
          exePath,
          default,
          IID_IShellItem,
        out var ppv);
      pShellItem = (IShellItem*)ppv;
      if (hr.Succeeded && pShellItem != null)
      {
        IShellItemImageFactory* pImageFactory = null;
        Guid iidImageFactory = IID_IShellItemImageFactory;
        hr = pShellItem->QueryInterface(&iidImageFactory, (void**)&pImageFactory);

        if (hr.Succeeded && pImageFactory != null)
        {
          DeleteObjectSafeHandle hBitmap = null;
          try
          {
            SIZE size = new SIZE(256, 256);
            pImageFactory->GetImage(size, SIIGBF.SIIGBF_BIGGERSIZEOK | SIIGBF.SIIGBF_ICONONLY, out hBitmap);

            if (hBitmap != null && !hBitmap.IsInvalid)
            {
              using (Bitmap bitmap = CreateBitmapFromHBitmap(hBitmap.DangerousGetHandle(), 256, 256))
              {
                if (bitmap != null)
                {
                  var finalBitmap = ProcessIcon(bitmap);
                  using (var ms = new MemoryStream())
                  {
                    finalBitmap.Save(ms, ImageFormat.Png);
                    byte[] pngBytes = ms.ToArray();

                    string appName = Path.GetFileNameWithoutExtension(exePath);
                    string fileName = $"{appName}.png";
                    return WidgetTool.SaveWidgetFileAsync("AppShortcut", fileName, pngBytes);
                  }
                }
              }
            }
          }
          finally
          {
            if (hBitmap != null && !hBitmap.IsInvalid)
              hBitmap.Dispose();
            if (pImageFactory != null)
              pImageFactory->Release();
            if (pShellItem != null)
              pShellItem->Release();
          }
        }
      }
      return null;
    }
    private Bitmap CreateBitmapFromHBitmap(nint hBitmap, int width, int height)
    {
      BITMAPINFOHEADER bmiHeader = new BITMAPINFOHEADER();
      bmiHeader.biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>();
      bmiHeader.biWidth = width;
      bmiHeader.biHeight = -height;
      bmiHeader.biPlanes = 1;
      bmiHeader.biBitCount = 32;
      bmiHeader.biCompression = 3; // BI_BITFIELDS
      bmiHeader.biSizeImage = (uint)(width * height * 4);

      int bufferSize = width * height * 4;
      byte[] bits = new byte[bufferSize];

      var hdc = PInvoke.GetDC(HWND.Null);

      unsafe
      {
        // 创建 BITMAPINFO 结构体，包含 BITMAPINFOHEADER
        BITMAPINFO bmi = new BITMAPINFO();
        bmi.bmiHeader = bmiHeader;

        fixed (byte* pBits = bits)
        {
          PInvoke.GetDIBits(
            hdc,
            new Microsoft.Win32.SafeHandles.SafeFileHandle(hBitmap, false),
            0,
            (uint)height,
            (void*)pBits,
            &bmi,
            0);
        }
      }
      PInvoke.ReleaseDC(HWND.Null, hdc);

      Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
      BitmapData data = result.LockBits(new Rectangle(0, 0, width, height),
                        ImageLockMode.WriteOnly,
                        PixelFormat.Format32bppArgb);
      Marshal.Copy(bits, 0, data.Scan0, bufferSize);
      result.UnlockBits(data);
      return result;
    }

    public Bitmap ProcessIcon(Bitmap originalIcon)
    {
      Bitmap upscaledIcon = originalIcon;
      Color[] dominantColors = ExtractDominantColors(upscaledIcon);
      Bitmap gradientBackground = GenerateGradient(256, 256, dominantColors[0], dominantColors[1]);

      Bitmap finalImage = ApplyGradientMask(upscaledIcon, gradientBackground);
      upscaledIcon.Dispose();
      gradientBackground.Dispose();
      return finalImage;
    }

    private Color[] ExtractDominantColors(Bitmap image)
    {
      BitmapData imageData = image.LockBits(
          new Rectangle(0, 0, image.Width, image.Height),
          ImageLockMode.ReadOnly, image.PixelFormat);

      Dictionary<Color, int> colorFrequency = new Dictionary<Color, int>();

      try
      {
        unsafe
        {
          byte* ptr = (byte*)imageData.Scan0;
          int bytesPerPixel = 4;
          int stride = imageData.Stride;

          for (int y = 0; y < image.Height; y++)
          {
            for (int x = 0; x < image.Width; x++)
            {
              byte alpha = ptr[y * stride + x * bytesPerPixel + 3];

              if (alpha > 128)
              {
                Color color = Color.FromArgb(
                    255,
                    ptr[y * stride + x * bytesPerPixel + 2],
                    ptr[y * stride + x * bytesPerPixel + 1],
                    ptr[y * stride + x * bytesPerPixel + 0]
                );

                Color simplifiedColor = SimplifyColor(color);

                if (colorFrequency.ContainsKey(simplifiedColor))
                {
                  colorFrequency[simplifiedColor]++;
                }
                else
                {
                  colorFrequency[simplifiedColor] = 1;
                }
              }
            }
          }
        }
      }
      finally
      {
        image.UnlockBits(imageData);
      }

      if (colorFrequency.Count == 0)
      {
        return new Color[] { Color.Gray, Color.DarkGray };
      }

      if (colorFrequency.Count == 1)
      {
        Color primaryColor = colorFrequency.Keys.First();
        Color secondaryColor = CreateContrastColor(primaryColor);
        return new Color[] { primaryColor, secondaryColor };
      }

      var sortedColors = colorFrequency.OrderByDescending(kvp => kvp.Value)
                                       .Take(2)
                                       .Select(kvp => kvp.Key)
                                       .ToArray();

      return sortedColors;
    }

    private Color SimplifyColor(Color color)
    {
      int r = (color.R / 32) * 32;
      int g = (color.G / 32) * 32;
      int b = (color.B / 32) * 32;

      r = Math.Min(r, 255);
      g = Math.Min(g, 255);
      b = Math.Min(b, 255);

      return Color.FromArgb(255, r, g, b);
    }

    private Color CreateContrastColor(Color color)
    {
      double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
      return luminance > 0.5 ? Color.FromArgb(255, 0, 0, 0) : Color.FromArgb(255, 255, 255, 255);
    }

    private Bitmap GenerateGradient(int width, int height, Color startColor, Color endColor)
    {
      Bitmap gradient = new Bitmap(width, height, PixelFormat.Format32bppArgb);
      using (Graphics g = Graphics.FromImage(gradient))
      {
        LinearGradientBrush brush = new LinearGradientBrush(
            new Rectangle(0, 0, width, height),
            startColor,
            endColor,
            LinearGradientMode.Vertical);

        g.FillRectangle(brush, 0, 0, width, height);
      }
      return gradient;
    }

    private Bitmap ApplyGaussianBlur(Bitmap source)
    {
      int width = source.Width;
      int height = source.Height;
      Bitmap blurred = new Bitmap(width, height, PixelFormat.Format32bppArgb);

      BitmapData srcData = source.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
      BitmapData dstData = blurred.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

      try
      {
        unsafe
        {
          byte* srcPtr = (byte*)srcData.Scan0;
          byte* dstPtr = (byte*)dstData.Scan0;
          int stride = srcData.Stride;
          int bytesPerPixel = 4;

          // Simple 3x3 Gaussian kernel
          float[] kernel = { 1/16f, 2/16f, 1/16f,
                            2/16f, 4/16f, 2/16f,
                            1/16f, 2/16f, 1/16f };

          for (int y = 1; y < height - 1; y++)
          {
            for (int x = 1; x < width - 1; x++)
            {
              float r = 0, g = 0, b = 0, a = 0;
              for (int ky = -1; ky <= 1; ky++)
              {
                for (int kx = -1; kx <= 1; kx++)
                {
                  int offset = (y + ky) * stride + (x + kx) * bytesPerPixel;
                  float weight = kernel[(ky + 1) * 3 + (kx + 1)];
                  r += srcPtr[offset + 2] * weight;
                  g += srcPtr[offset + 1] * weight;
                  b += srcPtr[offset + 0] * weight;
                  a += srcPtr[offset + 3] * weight;
                }
              }

              int dstOffset = y * stride + x * bytesPerPixel;
              dstPtr[dstOffset + 0] = (byte)Math.Min(Math.Max(b, 0), 255);
              dstPtr[dstOffset + 1] = (byte)Math.Min(Math.Max(g, 0), 255);
              dstPtr[dstOffset + 2] = (byte)Math.Min(Math.Max(r, 0), 255);
              dstPtr[dstOffset + 3] = (byte)Math.Min(Math.Max(a, 0), 255);
            }
          }
        }
      }
      finally
      {
        source.UnlockBits(srcData);
        blurred.UnlockBits(dstData);
      }
      return blurred;
    }

    private Bitmap ApplyGradientMask(Bitmap upscaledIcon, Bitmap gradientBackground)
    {
      // Apply blur to the icon
      Bitmap blurredIcon = ApplyGaussianBlur(upscaledIcon);

      int width = upscaledIcon.Width;
      int height = upscaledIcon.Height;
      Bitmap finalImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);

      // Create a lighting effect (radial gradient)
      Bitmap lightingEffect = new Bitmap(width, height, PixelFormat.Format32bppArgb);
      using (Graphics g = Graphics.FromImage(lightingEffect))
      {
        using (var path = new GraphicsPath())
        {
          path.AddEllipse(width * 0.1f, height * 0.1f, width * 0.8f, height * 0.8f);
          using (var brush = new PathGradientBrush(path))
          {
            brush.CenterColor = Color.FromArgb(100, 255, 255, 255); // Bright center
            brush.SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) }; // Transparent edge
            g.FillRectangle(Brushes.Transparent, 0, 0, width, height);
            g.FillPath(brush, path);
          }
        }
      }

      BitmapData iconData = blurredIcon.LockBits(
          new Rectangle(0, 0, width, height),
          ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
      BitmapData gradientData = gradientBackground.LockBits(
          new Rectangle(0, 0, width, height),
          ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
      BitmapData lightData = lightingEffect.LockBits(
          new Rectangle(0, 0, width, height),
          ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
      BitmapData finalData = finalImage.LockBits(
          new Rectangle(0, 0, width, height),
          ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

      try
      {
        unsafe
        {
          byte* iconPtr = (byte*)iconData.Scan0;
          byte* gradientPtr = (byte*)gradientData.Scan0;
          byte* lightPtr = (byte*)lightData.Scan0;
          byte* finalPtr = (byte*)finalData.Scan0;

          int bytesPerPixel = 4;
          int iconStride = iconData.Stride;
          int gradientStride = gradientData.Stride;
          int lightStride = lightData.Stride;
          int finalStride = finalData.Stride;

          for (int y = 0; y < height; y++)
          {
            for (int x = 0; x < width; x++)
            {
              int iconOffset = y * iconStride + x * bytesPerPixel;
              int gradientOffset = y * gradientStride + x * bytesPerPixel;
              int lightOffset = y * lightStride + x * bytesPerPixel;
              int finalOffset = y * finalStride + x * bytesPerPixel;

              byte iconAlpha = iconPtr[iconOffset + 3];

              if (iconAlpha < 128)
              {
                // Use gradient background for transparent areas
                float lightAlpha = lightPtr[lightOffset + 3] / 255f;
                finalPtr[finalOffset + 0] = (byte)(gradientPtr[gradientOffset + 0] * (1 - lightAlpha) + lightPtr[lightOffset + 0] * lightAlpha);
                finalPtr[finalOffset + 1] = (byte)(gradientPtr[gradientOffset + 1] * (1 - lightAlpha) + lightPtr[lightOffset + 1] * lightAlpha);
                finalPtr[finalOffset + 2] = (byte)(gradientPtr[gradientOffset + 2] * (1 - lightAlpha) + lightPtr[lightOffset + 2] * lightAlpha);
                finalPtr[finalOffset + 3] = 255;
              }
              else
              {
                // Blend icon with lighting effect
                float lightAlpha = lightPtr[lightOffset + 3] / 255f;
                finalPtr[finalOffset + 0] = (byte)(iconPtr[iconOffset + 0] * (1 - lightAlpha) + lightPtr[lightOffset + 0] * lightAlpha);
                finalPtr[finalOffset + 1] = (byte)(iconPtr[iconOffset + 1] * (1 - lightAlpha) + lightPtr[lightOffset + 1] * lightAlpha);
                finalPtr[finalOffset + 2] = (byte)(iconPtr[iconOffset + 2] * (1 - lightAlpha) + lightPtr[lightOffset + 2] * lightAlpha);
                finalPtr[finalOffset + 3] = iconAlpha;
              }
            }
          }
        }
      }
      finally
      {
        blurredIcon.UnlockBits(iconData);
        gradientBackground.UnlockBits(gradientData);
        lightingEffect.UnlockBits(lightData);
        finalImage.UnlockBits(finalData);
        blurredIcon.Dispose();
        lightingEffect.Dispose();
      }

      return finalImage;
    }
  }
}