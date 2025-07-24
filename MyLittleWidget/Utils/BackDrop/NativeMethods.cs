using static MyLittleWidget.Utils.BackDrop.NativeValues;

namespace MyLittleWidget.Utils.BackDrop;

public static partial class NativeMethods
{
  /// <summary>
  /// FlushMenuThemes clears the current theme settings for menus. It ensures that any changes to theme settings are
  /// applied immediately.
  /// </summary>
  [LibraryImport(ExternDll.UxTheme, EntryPoint = "#136", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
  public static partial void FlushMenuThemes();

  [LibraryImport(ExternDll.UxTheme, EntryPoint = "#135", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
  public static partial int SetPreferredAppMode(PreferredAppMode preferredAppMode);

  [LibraryImport(ExternDll.User32)]
  internal static partial IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam);

  [DllImport(ExternDll.User32)]
  [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
  internal static extern unsafe int FillRect(IntPtr hDC, ref Windows.Win32.Foundation.RECT lprc, Windows.Win32.Graphics.Gdi.HBRUSH hbr);

  // Import the 32-bit version of SetWindowLong for modifying MainWindow properties.
  [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
  internal static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

  // Import the 64-bit version of SetWindowLongPtr for modifying MainWindow properties.
  [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
  internal static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

  /// <summary>
  /// Sets a new value for a specified MainWindow attribute of a given MainWindow handle. The method adapts based on the
  /// architecture size.
  /// </summary>
  /// <param name="hWnd">Specifies the handle to the MainWindow whose attribute is being modified.</param>
  /// <param name="nIndex">Indicates the specific attribute to be changed for the MainWindow.</param>
  /// <param name="dwNewLong">Represents the new value to be set for the specified MainWindow attribute.</param>
  /// <returns>Returns the previous value of the specified MainWindow attribute.</returns>
  public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong) => IntPtr.Size == 4
      ? SetWindowLongPtr32(hWnd, nIndex, dwNewLong)
      : SetWindowLongPtr64(hWnd, nIndex, dwNewLong);

  [DllImport("CoreMessaging.dll")]
  internal static extern unsafe int CreateDispatcherQueueController(DispatcherQueueOptions options, IntPtr* instance);

  internal static HWND[]? EnumThreadWindows(Func<HWND, nint, bool> predicate, nint lParam)
  {
    var list = new List<HWND>();
    var handler = new WNDENUMPROC((_hWnd, _lParam) =>
    {
      try
      {
        if (predicate((HWND)_hWnd, _lParam)) list.Add((HWND)_hWnd);
      }
      catch { }

      return true;
    });

    EnumThreadWindows(PInvoke.GetCurrentThreadId(), handler, new LPARAM(lParam));
    return list.Count != 0 ? list.Distinct().ToArray() : Array.Empty<HWND>();
  }

  [DllImport("USER32.dll", ExactSpelling = true, PreserveSig = false)]
  internal static extern bool EnumThreadWindows([In] uint dwThreadId, [In] WNDENUMPROC lpfn, [In] nint lParam);

  [UnmanagedFunctionPointer(CallingConvention.Winapi)]
  internal delegate bool WNDENUMPROC([In] nint param0, [In] nint param1);
}