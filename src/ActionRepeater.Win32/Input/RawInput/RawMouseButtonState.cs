using System;

namespace ActionRepeater.Win32.Input;

/// <summary>
/// <para>The transition state of the mouse buttons.</para>
/// <para>Used by <see cref="RAWMOUSE.RawButtonData.RawButtonInfo"/>.</para>
/// </summary>
[Flags]
public enum RawMouseButtonState : ushort
{
    /// <summary>
    /// Left button changed to down.
    /// </summary>
    LEFT_BUTTON_DOWN = 0x0001,

    /// <summary>
    /// Left button changed to up.
    /// </summary>
    LEFT_BUTTON_UP = 0x0002,

    /// <summary>
    /// Right button changed to down.
    /// </summary>
    RIGHT_BUTTON_DOWN = 0x0004,

    /// <summary>
    /// Right button changed to up.
    /// </summary>
    RIGHT_BUTTON_UP = 0x0008,

    /// <summary>
    /// Middle button changed to down.
    /// </summary>
    MIDDLE_BUTTON_DOWN = 0x0010,

    /// <summary>
    /// Middle button changed to up.
    /// </summary>
    MIDDLE_BUTTON_UP = 0x0020,

    /// <summary>
    /// XBUTTON1 changed to down.
    /// </summary>
    XBUTTON1_DOWN = 0x0040,

    /// <summary>
    /// XBUTTON1 changed to up.
    /// </summary>
    XBUTTON1_UP = 0x0080,

    /// <summary>
    /// XBUTTON2 changed to down.
    /// </summary>
    XBUTTON2_DOWN = 0x0100,

    /// <summary>
    /// XBUTTON1 changed to up.
    /// </summary>
    XBUTTON2_UP = 0x0200,

    /// <summary>
    /// Raw input comes from a mouse wheel. The wheel delta is stored in <paramref name="usButtonData"/>.<br/>
    /// A positive value indicates that the wheel was rotated forward, away from the user; a negative value indicates that the wheel was rotated backward, toward the user.
    /// </summary>
    WHEEL = 0x0400,

    /// <summary>
    /// Raw input comes from a horizontal mouse wheel. The wheel delta is stored in <paramref name="usButtonData"/>.<br/>
    /// A positive value indicates that the wheel was rotated to the right; a negative value indicates that the wheel was rotated to the left.
    /// <br/><br/>
    /// <b>Windows XP/2000:</b> This value is not supported.
    /// </summary>
    HWHEEL = 0x0800,
}
