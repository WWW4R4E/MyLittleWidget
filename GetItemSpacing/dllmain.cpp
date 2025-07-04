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