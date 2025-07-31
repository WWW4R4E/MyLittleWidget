using ABI.Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
namespace MyLittleWidget.Contracts.AppShortcut
{

  public sealed partial class AppShortcutSettingPage : Page
  {

    internal AppDataPaths AppData = new();
    public event Action<WidgetConfig> ConfigurationSaved;
    private float _zoom = 1.0f;
    private float _iconOffsetX = 0f;
    private float _iconOffsetY = 0f;
    private float _shadowOffsetX = 14f;
    private float _shadowOffsetY = 14f;
    private float _shadowBlur = 10f;
    private float _shadowOpacity = 0.5f;
    private float _backgroundBlur = 40f;
    private CanvasBitmap _iconBitmap;
    private IWidgetToolService toolService;
    internal WidgetConfig WidgetConfig;

    public AppShortcutSettingPage(WidgetConfig widgetConfig, IWidgetToolService widgetToolService)
    {
      InitializeComponent();
      WidgetConfig = widgetConfig;
      toolService = widgetToolService;
    }

    private void ZoomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      _zoom = (float)(e.NewValue / 100.0);
      DrawCanvas.Invalidate();
    }

    private void IconOffsetXSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      _iconOffsetX = (float)e.NewValue;
      DrawCanvas.Invalidate();
    }

    private void IconOffsetYSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      _iconOffsetY = (float)e.NewValue;
      DrawCanvas.Invalidate();
    }

    private void ShadowOffsetXSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      _shadowOffsetX = (float)e.NewValue;
      DrawCanvas.Invalidate();
    }

    private void ShadowOffsetYSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      _shadowOffsetY = (float)e.NewValue;
      DrawCanvas.Invalidate();
    }

    private void ShadowBlurSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      _shadowBlur = (float)e.NewValue;
      DrawCanvas.Invalidate();
    }

    private void ShadowOpacitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      _shadowOpacity = (float)(e.NewValue / 100.0);
      DrawCanvas.Invalidate();
    }

    private void BackgroundBlurSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      _backgroundBlur = (float)e.NewValue;
      DrawCanvas.Invalidate();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
      var device = CanvasDevice.GetSharedDevice();
      var renderTarget = new CanvasRenderTarget(device, 512, 512, 96);

      using (var ds = renderTarget.CreateDrawingSession())
      {
        ds.Clear(Colors.Transparent);
        RenderIcon(renderTarget, ds, renderTarget.Size);
      }
      AppData.AppDisplayIcon = toolService.SaveWidgetFileAsync(WidgetConfig.WidgetType,Path.GetFileNameWithoutExtension(AppData.ApplicationPath)+".Png", Array.Empty<byte>());
      await renderTarget.SaveAsync(AppData.AppDisplayIcon, CanvasBitmapFileFormat.Png);
      WidgetConfig.CustomSettings = JsonSerializer.SerializeToElement(AppData);
      toolService.SaveWidegtDataAsync();
      ConfigurationSaved?.Invoke(WidgetConfig);
    }

    private void DrawCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
      if (_iconBitmap == null) return;
      RenderIcon(sender, args.DrawingSession, sender.Size);
    }
    private void RenderIcon(ICanvasResourceCreator resourceCreator, CanvasDrawingSession ds, Windows.Foundation.Size targetSize)
    {
      if (_iconBitmap == null) return;
      var center = targetSize.ToVector2() / 2.0f;
      Windows.UI.Color mainColor;
      using (var tinyTarget = new CanvasRenderTarget(resourceCreator, 1, 1, 96))
      {
        using (var lds = tinyTarget.CreateDrawingSession())
        {
          lds.DrawImage(_iconBitmap, new Windows.Foundation.Rect(0, 0, 1, 1));
        }

        var colors = tinyTarget.GetPixelColors();
        mainColor = colors.Length > 0 ? colors[0] : Colors.Gray;
      }


      var solidColorBaseEffect = new ColorSourceEffect
      {
        Color = mainColor
      };
      var compositeIconOnColorEffect = new CompositeEffect
      {
        Sources = { solidColorBaseEffect, _iconBitmap }
      };

      const float pixelationResolution = 48.0f;
      var sclar = pixelationResolution / (float)targetSize.Width;
      var downscaleEffect = new Transform2DEffect
      {
        Source = compositeIconOnColorEffect,
        TransformMatrix = Matrix3x2.CreateScale(sclar)
      };
      var upscaleEffect = new Transform2DEffect
      {
        Source = downscaleEffect,
        InterpolationMode = CanvasImageInterpolation.NearestNeighbor,
        TransformMatrix = Matrix3x2.CreateScale(1/sclar)
      };
      var gaussianBlurForBackground = new GaussianBlurEffect
      {
        Source = upscaleEffect,
        BlurAmount = _backgroundBlur,
        BorderMode = EffectBorderMode.Hard
      };
      var darkenEffect = new BrightnessEffect
      {
        Source = gaussianBlurForBackground,
        WhitePoint = new Vector2(1.0f, 0.7f),
        BlackPoint = new Vector2(0.0f, 0.0f)
      };

      ds.DrawImage(darkenEffect, Vector2.Zero);

      var scaleEffect = new Transform2DEffect
      {
        Source = _iconBitmap,
        TransformMatrix = Matrix3x2.CreateScale(_zoom)
      };
      var shadowBlurEffect = new GaussianBlurEffect
      {
        Source = scaleEffect,
        BlurAmount = _shadowBlur,
        BorderMode = EffectBorderMode.Hard
      };
      var finalShadowEffect = new TintEffect
      {
        Source = shadowBlurEffect,
        Color = Windows.UI.Color.FromArgb((byte)(_shadowOpacity * 255), 0, 0, 0)
      };

      var iconDrawSize = _iconBitmap.Size.ToVector2() * _zoom;
      var iconBasePosition = center - iconDrawSize / 2;
      var shadowDrawPosition = iconBasePosition + new Vector2(_iconOffsetX + _shadowOffsetX, _iconOffsetY + _shadowOffsetY);
      ds.DrawImage(finalShadowEffect, shadowDrawPosition);

      var iconPosition = iconBasePosition + new Vector2(_iconOffsetX, _iconOffsetY);
      var iconRect = new Windows.Foundation.Rect(iconPosition.ToPoint(), iconDrawSize.ToSize());
      ds.DrawImage(_iconBitmap, iconRect);
    }


    // 处理文件拖放
    private async void AppShortcutContent_Drop(object sender, DragEventArgs e)
    {
      if (e.DataView.Contains(StandardDataFormats.StorageItems))
      {
        var items = await e.DataView.GetStorageItemsAsync();
        if (items.Count > 0)
        {
          if (items[0] is StorageFile file && file.Path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
          {
            AppData.ApplicationPath = file.Path;
            await SetAppIconFromPath(AppData.ApplicationPath);
          }
        }
      }
      e.Handled = true;
    }

    private async Task SetAppIconFromPath(string filePath)
    {
      using var memoryStream = new MemoryStream();
      SaveIconFromExe(filePath).Result.Save(memoryStream, ImageFormat.Png);
      memoryStream.Position = 0;

      var randomAccessStream = new InMemoryRandomAccessStream();
      await RandomAccessStream.CopyAsync(memoryStream.AsInputStream(), randomAccessStream);
      randomAccessStream.Seek(0);

      _iconBitmap = await CanvasBitmap.LoadAsync(DrawCanvas, randomAccessStream);
      DrawCanvas.Invalidate();
    }


    private void AppShortcutContent_DragOver(object sender, DragEventArgs e)
    {
      e.AcceptedOperation = DataPackageOperation.Copy;
      e.Handled = true;
    }


    public static readonly Guid IID_IShellItem = new("43826d1e-e718-42ee-bc55-a1e261c37bfe");
    public static readonly Guid IID_IShellItemImageFactory = new("bcc18b79-ba16-442f-80c4-8a59c30c463b");

    private unsafe async Task<Bitmap> SaveIconFromExe(string filePath)
    {
      Windows.Win32.UI.Shell.IShellItem* pShellItem = null;
      Windows.Win32.UI.Shell.IShellItemImageFactory* pImageFactory = null;
      DeleteObjectSafeHandle hBitmap = null;

      try
      {
        Guid iidShellItemImageFactory = new Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b");
        HRESULT hr = PInvoke.SHCreateItemFromParsingName(filePath, default, ref iidShellItemImageFactory, out var ppv);
        pShellItem = (Windows.Win32.UI.Shell.IShellItem*)ppv;

        Guid iidImageFactory = IID_IShellItemImageFactory;
        hr = pShellItem->QueryInterface(&iidImageFactory, (void**)&pImageFactory);

        SIZE size = new SIZE(512, 512);
        pImageFactory->GetImage(size, Windows.Win32.UI.Shell.SIIGBF.SIIGBF_BIGGERSIZEOK | Windows.Win32.UI.Shell.SIIGBF.SIIGBF_ICONONLY, out hBitmap);
        return CreateBitmapFromHBitmap(hBitmap.DangerousGetHandle(), size);

      }
      finally
      {
        // 在 finally 块中释放所有资源
        if (hBitmap != null && !hBitmap.IsInvalid)
          hBitmap.Dispose();
        if (pImageFactory != null)
          pImageFactory->Release();
        if (pShellItem != null)
          pShellItem->Release();
      }
    }
    private Bitmap CreateBitmapFromHBitmap(nint hBitmap, SIZE size)
    {
      Bitmap result;
      var hdc = HDC.Null;
      try
      {
        BITMAPINFOHEADER bmiHeader = new BITMAPINFOHEADER();
        bmiHeader.biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>();
        bmiHeader.biWidth = size.Width;
        bmiHeader.biHeight = -size.Width;
        bmiHeader.biPlanes = 1;
        bmiHeader.biBitCount = 32;
        bmiHeader.biCompression = 3; // BI_BITFIELDS
        bmiHeader.biSizeImage = (uint)(size.Width * size.Width * 4);

        int bufferSize = (int)bmiHeader.biSizeImage;
        byte[] bits = new byte[bufferSize];

        hdc = PInvoke.GetDC(HWND.Null);
        unsafe
        {
          BITMAPINFO bmi = new BITMAPINFO();
          bmi.bmiHeader = bmiHeader;

          fixed (byte* pBits = bits)
          {
            int resultDib = PInvoke.GetDIBits(
              hdc,
              new Microsoft.Win32.SafeHandles.SafeFileHandle(hBitmap, false),
              0,
              (uint)size.Height,
              pBits,
              &bmi,
              0);
            if (resultDib == 0)
            {
              Debug.WriteLine($"[AppShortcut] GetDIBits 失败");
              return null; 
            }
          }
        }

        result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
        BitmapData data = result.LockBits(new Rectangle(0, 0, size.Width, size.Height),
                          ImageLockMode.WriteOnly,
                          PixelFormat.Format32bppArgb);
        Marshal.Copy(bits, 0, data.Scan0, bufferSize);
        result.UnlockBits(data);
      }
      finally
      {
        if (hdc != HDC.Null)
        {
          PInvoke.ReleaseDC(HWND.Null, hdc);
        }
      }
      return result;
    }

  }
  internal class AppDataPaths
  {
    public string ApplicationPath { get; set; } = string.Empty;
    public string ApplicationArguments { get; set; } = string.Empty;
    public string AppDisplayIcon { get; set; } = "ms-appx:///Assets/AppIcon.scale-400.png";
  }
  public static class Extensions
  {
    public static Vector2 ToVector2(this Windows.Foundation.Point p) => new((float)p.X, (float)p.Y);

    public static Windows.Foundation.Point ToPoint(this Vector2 v) => new(v.X, v.Y);

    public static Windows.Foundation.Size ToSize(this Vector2 v) => new(v.X, v.Y);
  }
}
