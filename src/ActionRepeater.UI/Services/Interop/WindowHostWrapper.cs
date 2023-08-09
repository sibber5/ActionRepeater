using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ActionRepeater.Core.Action;
using ActionRepeater.Win32;

namespace ActionRepeater.UI.Services.Interop;

public partial struct WindowHostWrapper : IDisposable
{
    public const string PathWindowsDll = "PathWindows.dll";

    public readonly bool IsWindowOpen => _pWindowHost != 0;

    private nint _pWindowHost;

    public void OpenPathWindow() => _pWindowHost = CreatePathWindow();

    public void OpenPathWindow(Span<POINT> points) => _pWindowHost = CreatePathWindow(points);

    public void OpenDrawablePathWindow(WindowClosingCallback windowClosingCallback) => _pWindowHost = CreateDrawablePathWindow(windowClosingCallback);

    public void OpenDrawablePathWindow(Span<MouseMovement> movements, WindowClosingCallback windowClosingCallback) => _pWindowHost = CreateDrawablePathWindow(movements, windowClosingCallback);

    public readonly nint GetPWindow() => GetPWindow(_pWindowHost);

    public void CloseWindow()
    {
        if (_pWindowHost == 0) return;

        HResult hr = DestroyPathWindow(_pWindowHost);

        DisposeDangerous(_pWindowHost);
        _pWindowHost = 0;

        VerifyHRAndWin32Err(hr);
    }

    public void Dispose()
    {
        if (_pWindowHost == 0) return;

        DisposeDangerous(_pWindowHost);
        _pWindowHost = 0;
    }

    private static unsafe nint CreatePathWindow()
    {
        nint ret = 0;

        VerifyHRAndWin32Err(CreatePathWindow(points: null, 0, &ret));

        return ret;
    }

    private static unsafe nint CreatePathWindow(Span<POINT> points)
    {
        nint ret = 0;
        HResult hr;

        fixed (POINT* pPoints = points)
        {
            hr = CreatePathWindow(pPoints, points.Length, &ret);
        }

        VerifyHRAndWin32Err(hr);

        return ret;
    }

    private static unsafe nint CreateDrawablePathWindow(WindowClosingCallback windowClosingCallback)
    {
        nint ret = 0;

        VerifyHRAndWin32Err(CreateDrawablePathWindow(null, 0, windowClosingCallback, &ret));

        return ret;
    }

    private static unsafe nint CreateDrawablePathWindow(Span<MouseMovement> points, WindowClosingCallback windowClosingCallback)
    {
        nint ret = 0;
        HResult hr;

        fixed (MouseMovement* pPoints = points)
        {
            hr = CreateDrawablePathWindow(pPoints, points.Length, windowClosingCallback, &ret);
        }

        VerifyHRAndWin32Err(hr);

        return ret;
    }

    private static void VerifyHRAndWin32Err(HResult hr)
    {
        if (MACROS.FAILED(hr))
        {
            throw new AggregateException(new COMException($"{hr} ({PathWindowsDll}).", (int)hr), new Win32Exception());
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void WindowClosingCallback(MouseMovement* points, int length);

    [LibraryImport(PathWindowsDll, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe partial HResult CreatePathWindow(POINT* points, int length, nint* ppWrapper);

    [LibraryImport(PathWindowsDll, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe partial HResult CreateDrawablePathWindow(MouseMovement* points, int length, WindowClosingCallback windowClosingCallback, nint* ppWrapper);

    [LibraryImport(PathWindowsDll)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial nint GetPWindow(nint pWrapper);

    [LibraryImport(PathWindowsDll, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial HResult DestroyPathWindow(nint pWrapper);

    [LibraryImport(PathWindowsDll)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial void DisposeDangerous(nint pWrapper);
}
