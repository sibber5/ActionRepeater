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

        if (MACROS.FAILED(hr)) throw new Win32Exception();
    }

    public void AddPoint(POINT point, bool render = true)
    {
        if (MACROS.FAILED(AddPointToPath(_pPathWindowWrapper, point, render))) throw new Win32Exception();
    }

    public unsafe void AddPoints(Span<POINT> points)
    {
        fixed (POINT* pPoints = &System.Runtime.InteropServices.Marshalling.SpanMarshaller<POINT, POINT>.ManagedToUnmanagedIn.GetPinnableReference(points))
        {
            if (MACROS.FAILED(AddPointsToPath(_pPathWindowWrapper, pPoints, points.Length))) throw new Win32Exception();
        }
    }

    public void ClearPath()
    {
        if (MACROS.FAILED(ClearPoints(_pPathWindowWrapper))) throw new Win32Exception();
    }

    public void RenderPath()
    {
        if (MACROS.FAILED(Render(_pPathWindowWrapper))) throw new Win32Exception();
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

        fixed (POINT* pPoints = &System.Runtime.InteropServices.Marshalling.SpanMarshaller<POINT, POINT>.ManagedToUnmanagedIn.GetPinnableReference(points))
        {
            hr = CreatePathWindow(pPoints, points.Length, &ret);
        }

        if (MACROS.FAILED(hr)) throw new Win32Exception();

        return ret;
    }

    private static unsafe nint CreatePathWindow()
    {
        nint ret = 0;

        if (MACROS.FAILED(CreatePathWindow(points: null, 0, &ret))) throw new Win32Exception();

        return ret;
    }

    [LibraryImport(PathWindowsDll, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static unsafe partial HResult CreatePathWindow(POINT* points, int length, nint* ppWrapper);

    [LibraryImport(PathWindowsDll, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial HResult DestroyPathWindow(nint pWrapper);

    [DllImport(PathWindowsDll, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    private static extern void DisposeDangerous(nint pWrapper);

    // LibraryImport requires disabling runtime marshalling, which disables ref and out params (among other things)
    //[LibraryImport(PathWindowsDll)]
    //[UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [DllImport(PathWindowsDll, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    private static extern HResult AddPointToPath(nint pWrapper, POINT point, [MarshalAs(UnmanagedType.I1)] bool render);

    [LibraryImport(PathWindowsDll, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static unsafe partial HResult AddPointsToPath(nint pWrapper, POINT* points, int length);

    [LibraryImport(PathWindowsDll, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial HResult ClearPoints(nint pWrapper);

    [LibraryImport(PathWindowsDll, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial HResult Render(nint pWrapper);
}
