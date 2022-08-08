using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ActionRepeater.Win32;

public static class SystemInformation
{
    public static IReadOnlyList<Rectangle> MonitorRects { get; }

    /// <summary>
    /// The time in ms since pressing the key afer which to start sending key repeat actions.
    /// </summary>
    public static int KeyRepeatDelayMS { get; }

    /// <summary>
    /// The time in ms between each key repeat input.
    /// </summary>
    public static double KeyRepeatInterval { get; }

    public static int KeyRepeatIntervalRounded { get; }

    static unsafe SystemInformation()
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


        // TODO: add calibration option to measure those values, because they may vary a lot depending on the hardware.

        // from docs: a value in the range from 0 (approximately 250 ms delay) through 3 (approximately 1 second delay)
        int keybdDelay;
        PInvoke.SystemParametersInfo(SystemParameter.GETKEYBOARDDELAY, 0, &keybdDelay, SystemParameterUpdateAction.None);
        KeyRepeatDelayMS = 250 * (keybdDelay + 1);

        // from docs: a value in the range from 0 (approximately 2.5 repetitions per second) through 31 (approximately 30 repetitions per second).
        uint keybdSpeed;
        PInvoke.SystemParametersInfo(SystemParameter.GETKEYBOARDSPEED, 0, &keybdSpeed, SystemParameterUpdateAction.None);
        double repitionsPerSec = Remap(keybdSpeed, 0, 31, 2.5, 30);
        KeyRepeatInterval = 1000 / repitionsPerSec;

        KeyRepeatIntervalRounded = (int)Math.Round(KeyRepeatInterval);

        static double Remap(double In, double inMin, double inMax, double outMin, double outMax)
        {
            return outMin + (In - inMin) * (outMax - outMin) / (inMax - inMin);
        }
    }
}
