namespace ActionRepeater.Win32.Input;

/// <summary>
/// <para>A set of bit flags that specify various aspects of mouse motion and button clicks. This can be any reasonable combination of it's values.</para>
/// <para>The bit flags that specify mouse button status are set to indicate changes in status, not ongoing conditions. For example, if the left mouse button is pressed and held down, <see cref="LEFTDOWN"/> is set when the left button is first pressed, but not for subsequent motions. Similarly <see cref="LEFTUP"/> is set only when the button is first released.</para>
/// <para>You cannot specify both the <see cref="WHEEL"/> (or <see cref="HWHEEL"/>) flag and either <see cref="XDOWN"/> or <see cref="XUP"/> flags simultaneously in the <paramref name="dwFlags"/> parameter, because they both require use of the <paramref name="mouseData"/> field.</para>
/// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/ns-winuser-mouseinput#members">Read more on docs.microsoft.com</see>.</para>
/// </summary>
[Flags]
public enum MOUSEEVENTF : uint
{
    /// <summary>
    /// Movement occurred.
    /// </summary>
    MOVE = 0x0001u,

    /// <summary>
    /// The left button was pressed.
    /// </summary>
    LEFTDOWN = 0x0002u,

    /// <summary>
    /// The left button was released.
    /// </summary>
    LEFTUP = 0x0004u,

    /// <summary>
    /// The right button was pressed.
    /// </summary>
    RIGHTDOWN = 0x0008u,

    /// <summary>
    /// The right button was released.
    /// </summary>
    RIGHTUP = 0x0010u,

    /// <summary>
    /// The middle button was pressed.
    /// </summary>
    MIDDLEDOWN = 0x0020u,

    /// <summary>
    /// The middle button was released.
    /// </summary>
    MIDDLEUP = 0x0040u,

    /// <summary>
    /// An X button (extra mouse [usually] side button) was pressed.
    /// </summary>
    XDOWN = 0x0080u,

    /// <summary>
    /// An X button (extra mouse [usually] side button) was released.
    /// </summary>
    XUP = 0x0100u,

    /// <summary>
    /// The wheel was moved, if the mouse has a wheel. The amount of movement is specified in <paramref name="mouseData"/>.
    /// </summary>
    WHEEL = 0x0800u,

    /// <summary>
    /// The wheel was moved horizontally, if the mouse has a wheel. The amount of movement is specified in <paramref name="mouseData"/>.<br/>
    /// <b>Windows XP/2000</b>: This value is not supported.
    /// </summary>
    HWHEEL = 0x1000u,

    /// <summary>
    /// The <b>WM_MOUSEMOVE</b> messages will not be coalesced. The default behavior is to coalesce <b>WM_MOUSEMOVE</b> messages.<br/>
    /// <b>Windows XP/2000</b>: This value is not supported.
    /// </summary>
    MOVE_NOCOALESCE = 0x2000u,

    /// <summary>
    /// Maps coordinates to the entire desktop. Must be used with <see cref="ABSOLUTE"/>.
    /// </summary>
    VIRTUALDESK = 0x4000u,

    /// <summary>
    /// The <paramref name="dx"/> and <paramref name="dy"/> members contain normalized absolute coordinates. If the flag is not set, <paramref name="dx"/> and <paramref name="dy"/> contain relative data (the change in position since the last reported position). This flag can be set, or not set, regardless of what kind of mouse or other pointing device, if any, is connected to the system. For further information about relative mouse motion, see the following Remarks section.
    /// </summary>
    ABSOLUTE = 0x8000u,
}
