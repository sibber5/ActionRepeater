using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ActionRepeater.Win32;

namespace ActionRepeater.UI.Services;

public partial struct PathWindowWrapper : IDisposable
{
    private const string PathWindowsDll = "PathWindows.dll";

    public bool IsWindowOpen => _pPathWindowWrapper != 0;

    private nint _pPathWindowWrapper;

    public void OpenWindow() => _pPathWindowWrapper = CreatePathWindow();

    public void OpenWindow(Span<POINT> points) => _pPathWindowWrapper = CreatePathWindow(points);

    public void CloseWindow()
    {
        if (_pPathWindowWrapper == 0) return;

        HResult hr = DestroyPathWindow(_pPathWindowWrapper);

        DisposeDangerous(_pPathWindowWrapper);
        _pPathWindowWrapper = 0;

        VerifyHRAndWin32Err(hr);
    }

    public void AddPoint(POINT point, bool render = true)
    {
        VerifyHR(AddPointToPath(_pPathWindowWrapper, point, render));
    }

    public unsafe void AddPoints(Span<POINT> points)
    {
        fixed (POINT* pPoints = points)
        {
            VerifyHR(AddPointsToPath(_pPathWindowWrapper, pPoints, points.Length));
        }
    }

    public void ClearPath()
    {
        VerifyHR(ClearPoints(_pPathWindowWrapper));
    }

    public void RenderPath()
    {
        VerifyHR(Render(_pPathWindowWrapper));
    }

    public void Dispose()
    {
        if (_pPathWindowWrapper == 0) return;

        DisposeDangerous(_pPathWindowWrapper);
        _pPathWindowWrapper = 0;
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

    private static unsafe nint CreatePathWindow()
    {
        nint ret = 0;

        VerifyHRAndWin32Err(CreatePathWindow(points: null, 0, &ret));

        return ret;
    }

    private static void VerifyHR(HResult hr)
    {
        if (MACROS.FAILED(hr))
        {
            throw new COMException($"{hr} ({PathWindowsDll}).", (int)hr);
        }
    }

    private static void VerifyHRAndWin32Err(HResult hr)
    {
        if (MACROS.FAILED(hr))
        {
            throw new AggregateException(new COMException($"{hr} ({PathWindowsDll}).", (int)hr), new Win32Exception());
        }
    }

    [LibraryImport(PathWindowsDll, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe partial HResult CreatePathWindow(POINT* points, int length, nint* ppWrapper);

    [LibraryImport(PathWindowsDll, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial HResult DestroyPathWindow(nint pWrapper);

    [LibraryImport(PathWindowsDll)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial void DisposeDangerous(nint pWrapper);

    // LibraryImport requires disabling runtime marshalling, which disables ref and out params (among other things)
    //[LibraryImport(PathWindowsDll)]
    //[UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [DllImport(PathWindowsDll, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    private static extern HResult AddPointToPath(nint pWrapper, POINT point, [MarshalAs(UnmanagedType.I1)] bool render);

    [LibraryImport(PathWindowsDll)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe partial HResult AddPointsToPath(nint pWrapper, POINT* points, int length);

    [LibraryImport(PathWindowsDll)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial HResult ClearPoints(nint pWrapper);

    [LibraryImport(PathWindowsDll)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial HResult Render(nint pWrapper);
}
