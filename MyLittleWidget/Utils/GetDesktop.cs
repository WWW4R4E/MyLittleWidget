using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace MyLittleWidget.Utils
{
    internal class GetDesktop
    {

        #region DllImport
        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr SelectObject(IntPtr hDC, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        internal static extern int BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("gdi32.dll")]
        internal static extern int DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        static extern int DeleteDC(nint hdc);

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("gdi32.dll")]
        static extern int GetBitmapBits(nint hbit, int cb, nint lpvBits);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        static extern int GetClientRect(nint hWnd, ref int lpRect);
        [Flags]
        public enum SendMessageTimeoutFlags : uint { SMTO_NORMAL = 0x0, }
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, SendMessageTimeoutFlags fuFlags, uint uTimeout, out IntPtr lpdwResult);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);


        const int LOGPIXELSX = 88;
        const int LOGPIXELSY = 90;
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

        internal const int SPI_GETDESKWALLPAPER = 0x0073;
        internal const int MAX_PATH = 260;
        #endregion


        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
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

   
        public static string GetWallpaperPath()
        {
            var sb = new StringBuilder(MAX_PATH);
            if (SystemParametersInfo(SPI_GETDESKWALLPAPER, sb.Capacity, sb, 0))
            {
                return sb.ToString();
            }
            return string.Empty;
        }
        public static float GetSystemDpiScale()
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            int dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
            ReleaseDC(IntPtr.Zero, hdc);
            return (float)(dpiX / 96.0); // 96为标准DPI
        }
        internal static  IntPtr GetTargetHwnd()
        {
            IntPtr progman = FindWindow("Progman", null);
             IntPtr shellDllDefView = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
            return shellDllDefView;
        }
        internal static unsafe SoftwareBitmap CaptureWindow()
        {
            var hWnd = GetTargetHwnd();
            var hdc = GetDC(hWnd);
            nint hdcMem = 0;
            nint hBitmap = 0;
            byte[]? bitmapData = null;

            try
            {
                hdcMem = CreateCompatibleDC(hdc);

                int* rect = stackalloc int[4];
                GetClientRect(hWnd, ref rect[0]);

                var left = rect[0];
                var top = rect[1];
                var width = rect[2] - rect[0];
                var height = rect[3] - rect[1];

                hBitmap = CreateCompatibleBitmap(hdc, width, height);
                var oldObj = SelectObject(hdcMem, hBitmap);

                BitBlt(hdcMem, 0, 0, width, height, hdc, left, top, 0x00CC0020);

                var count = width * height * 4;
                bitmapData = ArrayPool<byte>.Shared.Rent(count);

                fixed (byte* ptr = bitmapData)
                {
                    count = GetBitmapBits(hBitmap, count, (nint)ptr);
                }
                SelectObject(hdcMem, oldObj);

                var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Premultiplied);

                WriteBuffer(softwareBitmap, bitmapData.AsSpan(0, count));

                return softwareBitmap;
            }
            finally
            {
                if (hdc != 0) ReleaseDC(hWnd, hdc);
                if (hBitmap != 0) DeleteObject(hBitmap);
                if (hdcMem != 0) DeleteDC(hdcMem);
                if (bitmapData != null) ArrayPool<byte>.Shared.Return(bitmapData);
            }

            static void WriteBuffer(SoftwareBitmap softwareBitmap, Span<byte> bitmapData)
            {
                using (var lockBuffer = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
                using (var reference = lockBuffer.CreateReference())
                {
                    var desc = lockBuffer.GetPlaneDescription(0);
                    var width = desc.Width;
                    var height = desc.Height;
                    if (ComWrappers.TryGetComInstance(reference, out var punk))
                    {
                        var IID_IMemoryBufferByteAccess = new Guid("5b0d3235-4dba-4d44-865e-8f1d0e4fd04d");
                        if (Marshal.QueryInterface(punk, in IID_IMemoryBufferByteAccess, out var ppv) == 0)
                        {
                            try
                            {
                                byte* ptr = null;
                                uint capacity = 0;
                                if (((delegate* unmanaged[Stdcall]<nint, byte**, uint*, int>)(*(void***)ppv)[3])(ppv, &ptr, &capacity) == 0)
                                {
                                    for (int i = 0; i < height; i++)
                                    {
                                        bitmapData.Slice(i * width * 4, width * 4).CopyTo(new Span<byte>(ptr + desc.Stride * i, width * 4));
                                    }
                                }
                            }
                            finally
                            {
                                Marshal.Release(ppv);
                            }
                        }
                    }
                }
            }
        }
    }
}
