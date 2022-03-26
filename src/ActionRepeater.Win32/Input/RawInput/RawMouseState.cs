using System;

namespace ActionRepeater.Win32.Input;

/// <summary>
/// <para>The mouse state.</para>
/// <para>Used by <see cref="RAWMOUSE"/>.</para>
/// </summary>
[Flags]
public enum RawMouseState : ushort
{
    /// <summary>
    /// Mouse movement data is relative to the last mouse position.
    /// </summary>
    MOVE_RELATIVE = 0x00,

    /// <summary>
    /// Mouse movement data is based on absolute position.
    /// </summary>
    MOVE_ABSOLUTE = 0x01,

    /// <summary>
    /// Mouse coordinates are mapped to the virtual desktop (for a multiple monitor system).
    /// </summary>
    VIRTUAL_DESKTOP = 0x02,

    /// <summary>
    /// Mouse attributes changed; application needs to query the mouse attributes.
    /// </summary>
    ATTRIBUTES_CHANGED = 0x04,

    /// <summary>
    /// This mouse movement event was not coalesced. Mouse movement events can be coalesced by default.<br/>
    /// <b>Windows XP/2000:</b> This value is not supported.
    /// </summary>
    MOVE_NOCOALESCE = 0x08,
}
