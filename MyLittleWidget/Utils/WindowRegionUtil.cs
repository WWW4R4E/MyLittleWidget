using System.Diagnostics;
using static Windows.Win32.PInvoke; 

namespace MyLittleWidget.Utils
{
  public static class WindowRegionUtil
  {

    internal static void ApplySolidRegions(HWND windowHandle, IEnumerable<Rect> solidRects)
    {
      var hRgnMaster = CreateRectRgn(0, 0, 0, 0);
      if (hRgnMaster.IsNull)
      {
        return;
      }
      
      try
      {
        foreach (var rect in solidRects)
        {
          int left = (int)rect.X;
          int top = (int)rect.Y;
          int right = (int)(rect.X + rect.Width);
          int bottom = (int)(rect.Y + rect.Height);
      
          var hRgnTemp = CreateRectRgn(left, top, right, bottom);
          if (hRgnTemp.IsNull) continue; 
      
          try
          {
            CombineRgn(hRgnMaster, hRgnMaster, hRgnTemp, RGN_COMBINE_MODE.RGN_OR);
          }
          finally
          {
            DeleteObject(hRgnTemp);
          }
        }
      
        SetWindowRgn(windowHandle, hRgnMaster, true);
      }
      catch
      {
        if (!hRgnMaster.IsNull)
        {
          DeleteObject(hRgnMaster);
        }
        throw;
      }
      Debug.WriteLine("[TEST] Running TestWindowRegion_DigHalf...");
    }

    // 必要时恢复窗口形状 
    internal static void RestoreWindowShape(HWND windowHandle)
    {
      SetWindowRgn(windowHandle, null, true);
    }
  }
}