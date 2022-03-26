using System;

namespace ActionRepeater.Win32.Input;

/// <summary>
/// <para>Flags for scan code information.</para>
/// <para>Used by <see cref="RAWKEYBOARD"/>.</para>
/// </summary>
[Flags]
public enum RawInputKeyFlags : ushort
{
    /// <summary>
    /// The key is down.
    /// </summary>
    MAKE = 0,

    /// <summary>
    /// The key is up.
    /// </summary>
    BREAK = 1,

    /// <summary>
    /// The scan code has the E0 prefix.
    /// </summary>
    E0 = 2,

    /// <summary>
    /// The scan code has the E1 prefix.
    /// </summary>
    E1 = 4,
}
