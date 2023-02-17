namespace ActionRepeater.Win32.Input;

/// <summary>
/// <para>Contains information about a simulated keyboard event.</para>
/// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/ns-winuser-keybdinput#members">Read more on docs.microsoft.com</see>.</para>
/// </summary>
public struct KEYBDINPUT
{
    /// <summary>
    /// <para>Type: <b>WORD</b> A <a href="https://docs.microsoft.com/windows/desktop/inputdev/virtual-key-codes">virtual-key code</a>. The code must be a value in the range 1 to 254. If the <see cref="dwFlags"/> member specifies <see cref="KEYEVENTF.UNICODE"/>, <see cref="wVk"/> must be <i>0</i>.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/ns-winuser-keybdinput#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public VirtualKey wVk;
    /// <summary>
    /// <para>Type: <b>WORD</b> A hardware scan code for the key. If <see cref="dwFlags"/> specifies <see cref="KEYEVENTF.UNICODE"/>, <see cref="wScan"/> specifies a Unicode character which is to be sent to the foreground application.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/ns-winuser-keybdinput#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public ushort wScan;
    /// <summary>
    /// <para>Type: <b>DWORD</b> Specifies various aspects of a keystroke. This can be certain combinations of <see cref="KEYEVENTF"/>.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/ns-winuser-keybdinput#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public KEYEVENTF dwFlags;
    /// <summary>
    /// <para>Type: <b>DWORD</b> The time stamp for the event, in milliseconds. If this parameter is zero, the system will provide its own time stamp.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/ns-winuser-keybdinput#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public uint time;
    /// <summary>
    /// <para>Type: <b>ULONG_PTR</b> An additional value associated with the keystroke. Use the <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getmessageextrainfo">GetMessageExtraInfo</a> function to obtain this information.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/ns-winuser-keybdinput#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public nuint dwExtraInfo;
}
