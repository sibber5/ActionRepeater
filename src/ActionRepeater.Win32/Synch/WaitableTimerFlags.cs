using System;

namespace ActionRepeater.Win32.Synch;

[Flags]
public enum WaitableTimerFlags : uint
{
    None = 0u,
    /// <summary>
    /// The timer must be manually reset. Otherwise, the system automatically resets the timer after releasing a single waiting thread.
    /// </summary>
    MANUAL_RESET = 0x00000001u,
    /// <summary>
    /// <para>Creates a high resolution timer. Use this value for time-critical situations when short expiration delays on the order of a few milliseconds are unacceptable.</para>
    /// <para>This value is supported in Windows 10, version 1803, and later.</para>
    /// </summary>
    HIGH_RESOLUTION = 0x00000002,
}
