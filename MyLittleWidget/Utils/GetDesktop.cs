using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Windows.Graphics.Imaging;
using WinRT;

namespace MyLittleWidget.Utils
{
  internal class GetDesktop
  {
    public const uint PW_RENDERFULLCONTENT = 0x00000002;

    internal const int SPI_GETDESKWALLPAPER = 0x0073;
    internal const int MAX_PATH = 260;

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
      public int X;
      public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GridInfo
    {
      public POINT spacing;
      public POINT grid;
      public RECT rcWorkArea;
    }

    [DllImport("GetItemSpacing.dll")]
    internal static extern GridInfo GetDesktopGridInfo();

    public static float GetSystemDpiScale()
    {
      var hdc = PInvoke.GetDC(HWND.Null);
      int dpiX = PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.LOGPIXELSX);
      PInvoke.ReleaseDC(HWND.Null, hdc);
      return (float)(dpiX / 96.0); // 96为标准DPI
    }

    public static unsafe SoftwareBitmap CaptureWindow()
    {
      // 1. 获取顶层窗口句柄
      HWND hwnd = (HWND)WindowNative.GetWindowHandle(((App)App.Current).childWindow);

      // 2. 获取窗口尺寸
      PInvoke.GetWindowRect(hwnd, out RECT rect);
      int width = rect.right - rect.left;
      int height = rect.bottom - rect.top;

      // 3. 创建 GDI 对象用于绘图
      var hdcScreen = PInvoke.GetWindowDC(hwnd);
      var hdcMem = PInvoke.CreateCompatibleDC(hdcScreen);
      var hBitmap = PInvoke.CreateCompatibleBitmap(hdcScreen, width, height);
      var hOldBitmap = PInvoke.SelectObject(hdcMem, hBitmap);

      try
      {
        bool success = PInvoke.PrintWindow(hwnd, hdcMem, (PRINT_WINDOW_FLAGS)PW_RENDERFULLCONTENT);
        if (success)
        {
          using (Bitmap bmp = Bitmap.FromHbitmap(hBitmap))
          {
            var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, bmp.Width, bmp.Height, BitmapAlphaMode.Premultiplied);

            // 锁定源和目标的内存
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            using (var buffer = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
            using (var reference = buffer.CreateReference())
            {
              try
              {
                byte* destPtr;
                uint capacity;
                reference.As<IMemoryBufferByteAccess>().GetBuffer(out destPtr, out capacity);

                var desc = buffer.GetPlaneDescription(0);

                // 逐行复制数据，以防 stride（行距）不同
                for (int i = 0; i < bmp.Height; i++)
                {
                  var sourceLine = new Span<byte>((void*)(bmpData.Scan0 + i * bmpData.Stride), bmp.Width * 4);
                  var destLine = new Span<byte>(destPtr + i * desc.Stride, bmp.Width * 4);
                  sourceLine.CopyTo(destLine);
                }
              }
              finally
              {
                bmp.UnlockBits(bmpData);
              }
            }
            return softwareBitmap;
          }
        }
        else
        {
          Debug.WriteLine("PrintWindow failed.");
          throw new InvalidOperationException("PrintWindow failed to capture the window.");
        }
      }
      finally
      {
        PInvoke.SelectObject((HDC)hdcMem, (HGDIOBJ)hOldBitmap);
        PInvoke.DeleteObject((HGDIOBJ)hBitmap);
        PInvoke.DeleteDC((HDC)hdcMem);
        PInvoke.ReleaseDC(hwnd, (HDC)hdcScreen);
      }
    }

    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private unsafe interface IMemoryBufferByteAccess
    {
      void GetBuffer(out byte* buffer, out uint capacity);
    }
  }
}