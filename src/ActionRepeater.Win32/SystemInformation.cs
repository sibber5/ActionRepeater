using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ActionRepeater.Win32;

public static class SystemInformation
{
    public static Rectangle VirtualScreen { get; private set; }

    public static Rectangle PrimaryMonitorRect { get; private set; }

    public static IReadOnlyList<Rectangle> MonitorRects { get; private set; } = null!;

    /// <summary>
    /// The time in ms since pressing the key afer which to start sending key repeat actions.
    /// </summary>
    public static int KeyRepeatDelayMS { get; private set; }

    /// <summary>
    /// The time in ms between each key repeat input.
    /// </summary>
    public static double KeyRepeatIntervalMS { get; private set; }

    public static int KeyRepeatIntervalRoundedMS { get; private set; }

    public static bool IsMouseAccelerationEnabled { get; private set; }

    static unsafe SystemInformation()
    {
        RefreshMonitorSettings();
        RefreshKeyboardSettings();
        RefreshMonitorSettings();
    }

    public static void RefreshMonitorSettings()
    {
        int virtScreenWidth = PInvoke.GetSystemMetrics(SystemMetric.CXVIRTUALSCREEN);
        int virtScreenHeight = PInvoke.GetSystemMetrics(SystemMetric.CYVIRTUALSCREEN);
        VirtualScreen = new(default, new(virtScreenWidth, virtScreenHeight));

        int primaryMonitorWidth = PInvoke.GetSystemMetrics(SystemMetric.CXSCREEN);
        int primaryMonitorHeight = PInvoke.GetSystemMetrics(SystemMetric.CYSCREEN);
        PrimaryMonitorRect = new(default, new(primaryMonitorWidth, primaryMonitorHeight));

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

    public static unsafe void RefreshKeyboardSettings()
    {
        // TODO: add calibration option to measure those values, because they may vary a lot depending on the hardware.

        // from docs: a value in the range from 0 (approximately 250 ms delay) through 3 (approximately 1 second delay)
        int keybdDelay;
        PInvoke.SystemParametersInfo(SystemParameter.GETKEYBOARDDELAY, 0, &keybdDelay, SystemParameterUpdateAction.None);
        KeyRepeatDelayMS = 250 * (keybdDelay + 1);

        // from docs: a value in the range from 0 (approximately 2.5 repetitions per second) through 31 (approximately 30 repetitions per second).
        uint keybdSpeed;
        PInvoke.SystemParametersInfo(SystemParameter.GETKEYBOARDSPEED, 0, &keybdSpeed, SystemParameterUpdateAction.None);
        double repitionsPerSec = Remap(keybdSpeed, 0, 31, 2.5, 30);
        KeyRepeatIntervalMS = 1000 / repitionsPerSec;

        KeyRepeatIntervalRoundedMS = (int)Math.Round(KeyRepeatIntervalMS);

        static double Remap(double In, double inMin, double inMax, double outMin, double outMax)
        {
            return outMin + (In - inMin) * (outMax - outMin) / (inMax - inMin);
        }
    }

    public static unsafe void RefreshMouseSettings()
    {
        void* mouseSettings = NativeMemory.AllocZeroed(3, sizeof(int));
        PInvoke.SystemParametersInfo(SystemParameter.GETMOUSE, 0, mouseSettings, SystemParameterUpdateAction.None);
        int acceleration = ((int*)mouseSettings)[2];
        NativeMemory.Free(mouseSettings);
        IsMouseAccelerationEnabled = acceleration != 0;
    }

    public static POINT GetVirtScreenPosFromPosRelToPrimary(POINT posRelToPrimaryOrigin)
    {
        int x = posRelToPrimaryOrigin.x - PrimaryMonitorRect.Width + VirtualScreen.Width;
        int y = posRelToPrimaryOrigin.y - PrimaryMonitorRect.Height + VirtualScreen.Height;
        return new(x, y);
    }

    public static POINT GetAbsoluteCoordinateFromVirtScreenPoint(POINT virtScreenPoint)
    {
        int x = (int)MathF.Round(((float)virtScreenPoint.x / VirtualScreen.Width) * 65535);
        int y = (int)MathF.Round(((float)virtScreenPoint.y / VirtualScreen.Height) * 65535);
        return new(x, y);
    }

    public static POINT GetAbsoluteCoordinateFromPosRelToPrimary(POINT posRelToPrimaryOrigin)
    {
        POINT virtScreenPoint = GetVirtScreenPosFromPosRelToPrimary(posRelToPrimaryOrigin);
        return GetAbsoluteCoordinateFromVirtScreenPoint(virtScreenPoint);
    }
}
