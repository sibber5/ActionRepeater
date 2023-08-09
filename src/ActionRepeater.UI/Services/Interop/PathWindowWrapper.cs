using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ActionRepeater.Win32;

namespace ActionRepeater.UI.Services.Interop;

public partial struct PathWindowWrapper : IDisposable
{
    public readonly bool IsWindowOpen => _windowHost.IsWindowOpen;

    private WindowHostWrapper _windowHost;

    public void OpenWindow() => _windowHost.OpenPathWindow();

    public void OpenWindow(Span<POINT> points) => _windowHost.OpenPathWindow(points);

    public void CloseWindow() => _windowHost.CloseWindow();

    public readonly void AddPoint(POINT point, bool render = true) => VerifyHR(AddPointToPath(_windowHost.GetPWindow(), point, render));

    public readonly unsafe void AddPoints(Span<POINT> points)
    {
        fixed (POINT* pPoints = points)
        {
            VerifyHR(AddPointsToPath(_windowHost.GetPWindow(), pPoints, points.Length));
        }
    }

    public readonly void ClearPath() => VerifyHR(ClearPoints(_windowHost.GetPWindow()));

    public readonly void RenderPath() => VerifyHR(Render(_windowHost.GetPWindow()));

    public void Dispose() => _windowHost.Dispose();

    private static void VerifyHR(HResult hr)
    {
        if (MACROS.FAILED(hr))
        {
            throw new COMException($"{hr} ({WindowHostWrapper.PathWindowsDll}).", (int)hr);
        }
    }

    // LibraryImport requires disabling runtime marshalling, which disables ref and out params (among other things)
    //[LibraryImport(PathWindowsDll)]
    //[UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [DllImport(WindowHostWrapper.PathWindowsDll, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    private static extern HResult AddPointToPath(nint pPathWindow, POINT point, [MarshalAs(UnmanagedType.I1)] bool render);

    [LibraryImport(WindowHostWrapper.PathWindowsDll)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe partial HResult AddPointsToPath(nint pPathWindow, POINT* points, int length);

    [LibraryImport(WindowHostWrapper.PathWindowsDll)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial HResult ClearPoints(nint pPathWindow);

    [LibraryImport(WindowHostWrapper.PathWindowsDll)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial HResult Render(nint pPathWindow);
}
