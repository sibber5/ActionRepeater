namespace ActionRepeater.Win32.Input;

/// <summary>Contains information about a simulated mouse event.</summary>
/// <remarks>
/// <para>If the mouse has moved, indicated by <b>MOUSEEVENTF_MOVE</b>, <b>dx</b> and <b>dy</b> specify information about that movement. The information is specified as absolute or relative integer values.</para>
/// <para>If <b>MOUSEEVENTF_ABSOLUTE</b> value is specified, <b>dx</b> and <b>dy</b> contain normalized absolute coordinates between 0 and 65,535. The event procedure maps these coordinates onto the display surface. Coordinate (0,0) maps onto the upper-left corner of the display surface; coordinate (65535,65535) maps onto the lower-right corner. In a multimonitor system, the coordinates map to the primary monitor.</para>
/// <para>If <b>MOUSEEVENTF_VIRTUALDESK</b> is specified, the coordinates map to the entire virtual desktop.</para>
/// <para>If the <b>MOUSEEVENTF_ABSOLUTE</b> value is not specified, <b>dx</b>and <b>dy</b> specify movement relative to the previous mouse event (the last reported position). Positive values mean the mouse moved right (or down); negative values mean the mouse moved left (or up).</para>
/// <para>Relative mouse motion is subject to the effects of the mouse speed and the two-mouse threshold values. A user sets these three values with the <b>Pointer Speed</b> slider of the Control Panel's <b>Mouse Properties</b> sheet. You can obtain and set these values using the <see href="https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-systemparametersinfoa">SystemParametersInfo</see> function.</para>
/// <para>The system applies two tests to the specified relative mouse movement. If the specified distance along either the x or y axis is greater than the first mouse threshold value, and the mouse speed is not zero, the system doubles the distance. If the specified distance along either the x or y axis is greater than the second mouse threshold value, and the mouse speed is equal to two, the system doubles the distance that resulted from applying the first threshold test. It is thus possible for the system to multiply specified relative mouse movement along the x or y axis by up to four times.</para>
/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-mouseinput#">Read more on docs.microsoft.com</see>.</para>
/// </remarks>
public struct MOUSEINPUT
{
	/// <summary>
	/// <para>Type: <b>LONG</b> The absolute position of the mouse, or the amount of motion since the last mouse event was generated, depending on the value of the <paramref name="dwFlags"/> member. Absolute data is specified as the x coordinate of the mouse; relative data is specified as the number of pixels moved.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-mouseinput#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public int dx;
	/// <summary>
	/// <para>Type: <b>LONG</b> The absolute position of the mouse, or the amount of motion since the last mouse event was generated, depending on the value of the <paramref name="dwFlags"/> member. Absolute data is specified as the y coordinate of the mouse; relative data is specified as the number of pixels moved.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-mouseinput#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public int dy;
	/// <summary>
	/// <para>Type: <b>DWORD</b> If <paramref name="dwFlags"/> contains <see cref="MOUSEEVENTF.WHEEL"/>, then <paramref name="mouseData"/> specifies the amount of wheel movement. A positive value indicates that the wheel was rotated forward, away from the user; a negative value indicates that the wheel was rotated backward, toward the user. One wheel click is defined as <see cref="WHEEL_DELTA"/>, which is <i>120</i>. use <see cref="CalculateWheelDeltaUInt32"/>.</para>
	/// <para>Windows Vista: If <paramref name="dwFlags"/> contains <see cref="MOUSEEVENTF.HWHEEL"/>, then <paramref name="mouseData"/> specifies the amount of wheel movement. A positive value indicates that the wheel was rotated to the right; a negative value indicates that the wheel was rotated to the left. One wheel click is defined as <see cref="WHEEL_DELTA"/>, which is <i>120</i>. use <see cref="CalculateWheelDeltaUInt32"/>.</para>
	/// <para>If <paramref name="dwFlags"/> does not contain <see cref="MOUSEEVENTF.WHEEL"/>, <see cref="MOUSEEVENTF.HWHEEL"/>, <see cref="MOUSEEVENTF.XDOWN"/>, or <see cref="MOUSEEVENTF.XUP"/>, then <paramref name="mouseData"/> should be zero.</para>
	/// <para>If <paramref name="dwFlags"/> contains <see cref="MOUSEEVENTF.XDOWN"/> or <see cref="MOUSEEVENTF.XUP"/>, then <paramref name="mouseData"/> specifies which X buttons were pressed or released. This value may be any combination of <see cref="XBUTTON1"/> and <see cref="XBUTTON2"/></para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-mouseinput#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint mouseData;
	/// <summary>Type: <b>DWORD</b></summary>
	public MOUSEEVENTF dwFlags;
	/// <summary>
	/// <para>Type: <b>DWORD</b> The time stamp for the event, in milliseconds. If this parameter is 0, the system will provide its own time stamp.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-mouseinput#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint time;
	/// <summary>
	/// <para>Type: <b>ULONG_PTR</b> An additional value associated with the mouse event. An application calls <see href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getmessageextrainfo">GetMessageExtraInfo</see> to obtain this extra information.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-mouseinput#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public nuint dwExtraInfo;

	/// <summary>
	/// Set if the first X button is pressed or released.
	/// </summary>
	public const ushort XBUTTON1 = 0x0001;
	/// <summary>
	/// Set if the second X button is pressed or released.
	/// </summary>
	public const ushort XBUTTON2 = 0x0002;
	/// <summary>
	/// <b>WHEEL_DELTA</b> constant. if you want more than one click forward, or one or more click backward, use <see cref="CalculateWheelDeltaUInt32"/> or <see cref="CalculateWheelDeltaUInt16"/>
	/// </summary>
	public const uint WHEEL_DELTA = 120;
	/// <param name="param">Either <see cref="mouseData"/> or <paramref name="wParam"/> of WM_MOUSEWHEEL.</param>
	public static short GET_WHEEL_DELTA(nuint param) => unchecked((short)MACROS.HIWORD(param));
	/// <param name="param">Either <see cref="mouseData"/> or <paramref name="wParam"/> of WM_MOUSEWHEEL.</param>
	public static ushort GET_XBUTTON(nuint param) => MACROS.HIWORD(param);
	/// <param name="steps">
	/// The number of wheel clicks.<br/>
	/// A positive value indicates that the wheel was rotated forward/to the right, away from the user;
	/// a negative value indicates that the wheel was rotated backward/to the left, toward the user.
	/// </param>
	/// <returns><see cref="ushort"/> that represents the amount of wheel movement specified.</returns>
	public static ushort CalculateWheelDeltaUInt16(int steps) => unchecked((ushort)(120 * steps));
	/// <param name="steps">
	/// <inheritdoc cref="CalculateWheelDeltaUInt16"/> 
	/// </param>
	/// <returns><see cref="uint"/> that represents the amount of wheel movement specified.</returns>
	public static uint CalculateWheelDeltaUInt32(int steps) => unchecked((uint)(120 * steps));
}
