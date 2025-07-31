using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;

namespace MyLittleWidget.Utils
{
  public static class WindowRegionUtil
  {
    /// <summary>
    /// 为窗口应用一组圆角矩形区域（圆角半径18像素）。
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <param name="solidRects">矩形区域集合</param>
    /// <exception cref="ArgumentNullException">当 windowHandle 或 solidRects 为空时抛出</exception>
    /// <exception cref="InvalidOperationException">当 GDI 操作失败时抛出</exception>
    public static void ApplySolidRegions(HWND windowHandle, IEnumerable<Rect> solidRects)
    {
      if (windowHandle.IsNull)
      {
        throw new ArgumentNullException(nameof(windowHandle), "窗口句柄不能为空");
      }
      if (solidRects == null)
      {
        throw new ArgumentNullException(nameof(solidRects), "矩形区域集合不能为空");
      }

      // 创建主区域
      HRGN hRgnMaster = PInvoke.CreateRectRgn(0, 0, 0, 0);
      if (hRgnMaster.IsNull)
      {
        throw new InvalidOperationException("创建主区域失败");
      }

      bool hasRegions = false;
      try
      {
        foreach (var rect in solidRects)
        {
          if (rect.Width <= 0 || rect.Height <= 0)
          {
            Debug.WriteLine($"[WindowRegionUtil] 跳过无效矩形 (X={rect.X}, Y={rect.Y}, Width={rect.Width}, Height={rect.Height})");
            continue;
          }

          // 计算圆角矩形坐标，添加3像素偏移
          int left = (int)rect.X + 3;
          int top = (int)rect.Y + 3;
          int right = (int)(rect.X + rect.Width) + 3;
          int bottom = (int)(rect.Y + rect.Height) + 3;

          // 创建圆角矩形区域（圆角半径18像素）
          HRGN hRgnTemp =PInvoke.CreateRoundRectRgn(left, top, right, bottom, 36, 36);
          if (hRgnTemp.IsNull)
          {
            Debug.WriteLine($"[WindowRegionUtil] 创建圆角矩形区域失败 (X={rect.X}, Y={rect.Y}, Width={rect.Width}, Height={rect.Height})");
            continue;
          }

          try
          {
            // 合并区域，使用 RGN_OR（并集）
            var result = PInvoke.CombineRgn(hRgnMaster, hRgnMaster, hRgnTemp, RGN_COMBINE_MODE.RGN_OR);
            if (result == GDI_REGION_TYPE.RGN_ERROR )
            {
              continue;
            }
            hasRegions = true;
          }

          finally
          {
            PInvoke.DeleteObject(hRgnTemp);
          }
        }
       PInvoke.SetWindowRgn(windowHandle, hRgnMaster, true);
      }
      finally
      {
        if (!hRgnMaster.IsNull)
        {
          PInvoke.DeleteObject(hRgnMaster);
        }
      }
    }

    /// <summary>
    /// 恢复窗口的默认形状（移除自定义区域）。
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <exception cref="ArgumentNullException">当 windowHandle 为空时抛出</exception>
    internal static void RestoreWindowShape(HWND windowHandle)
    {
      PInvoke.SetWindowRgn(windowHandle, null, true);
    }
  }
}