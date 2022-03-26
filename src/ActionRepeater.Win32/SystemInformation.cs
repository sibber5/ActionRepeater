using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ActionRepeater.Win32;

public static class SystemInformation
{
    public static IReadOnlyList<Rectangle> MonitorRects { get; }

    static SystemInformation()
    {
        List<Rectangle> monitors = new();

        PInvoke.EnumDisplayMonitors(
            IntPtr.Zero,
            IntPtr.Zero,
            (hMonitor, hdcm, pRect, lParam) =>
            {
                var rect = Marshal.PtrToStructure<RECT>(pRect);
                Marshal.DestroyStructure<RECT>(pRect);
                Marshal.FreeHGlobal(pRect);

                monitors.Add(rect);

                return true;
            },
            default);

        MonitorRects = monitors;
    }
}
