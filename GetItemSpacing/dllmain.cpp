#include "pch.h"
#include <Windows.h>
#include <CommCtrl.h> 
#include <iostream>

typedef struct {
    POINT spacing;
    POINT grid;
    RECT rcWorkArea;
} GridInfo;

extern "C" __declspec(dllexport)
GridInfo GetDesktopGridInfo() {
    GridInfo info = {}; 

    HWND hDesktop = GetShellWindow();
    if (!hDesktop) {
        std::wcerr << L"Failed to get shell window handle." << std::endl;
        return info;
    }
    HWND hDefView = FindWindowExW(hDesktop, NULL, L"SHELLDLL_DefView", NULL);
    if (!hDefView) {
        std::wcerr << L"Failed to find SHELLDLL_DefView." << std::endl;
        return info;
    }
    HWND hListView = FindWindowExW(hDefView, NULL, L"SysListView32", L"FolderView");
    if (!hListView) {
        std::wcerr << L"Failed to get desktop ListView handle." << std::endl;
        return info;
    }

    SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

    DWORD itemSpacing = ListView_GetItemSpacing(hListView, FALSE);
    if (itemSpacing == 0) {
        std::cerr << "Failed to get item spacing via ListView_GetItemSpacing." << std::endl;
        return info;
    }

    info.spacing.x = LOWORD(itemSpacing);
    info.spacing.y = HIWORD(itemSpacing);

    if (!SystemParametersInfo(SPI_GETWORKAREA, 0, &info.rcWorkArea, 0)) {
        std::wcerr << L"SystemParametersInfo failed, falling back to GetSystemMetrics." << std::endl;
        info.rcWorkArea.left = 0;
        info.rcWorkArea.top = 0;
        info.rcWorkArea.right = GetSystemMetrics(SM_CXSCREEN);
        info.rcWorkArea.bottom = GetSystemMetrics(SM_CYSCREEN);
    }
    int effectiveWidth = info.rcWorkArea.right - info.rcWorkArea.left;
    int effectiveHeight = info.rcWorkArea.bottom - info.rcWorkArea.top;
    int cols = effectiveWidth / info.spacing.x;
    int rows = effectiveHeight / info.spacing.y;
    info.grid = { cols, rows };
    return info;
}
extern "C" __declspec(dllexport)
BOOL sws_WindowHelpers_EnsureWallpaperHWND()
{
    // See: https://github.com/valinet/ExplorerPatcher/issues/525
    HWND progman = GetShellWindow();
    if (progman)
    {
        DWORD_PTR res0 = 0, res1 = 0, res2 = 0, res3 = 0;
        SendMessageTimeoutW(progman, 0x052C, 0xA, 0, SMTO_NORMAL, 1000, &res0);
        if (FAILED(res0))
        {
            return FALSE;
        }
        SendMessageTimeoutW(progman, 0x052C, 0xD, 0, SMTO_NORMAL, 1000, &res1);
        SendMessageTimeoutW(progman, 0x052C, 0XD, 1, SMTO_NORMAL, 1000, &res2);
        SendMessageTimeoutW(progman, 0x052C, 0, 0, SMTO_NORMAL, 1000, &res3);  
        return !res1 && !res2 && !res3;
    }
    return FALSE;
}