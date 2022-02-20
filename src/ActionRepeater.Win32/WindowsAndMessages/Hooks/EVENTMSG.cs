namespace ActionRepeater.Win32.WindowsAndMessages;

/// <summary>Contains information about a hardware message sent to the system message queue. This structure is used to store message information for the JournalPlaybackProc callback function.</summary>
/// <remarks>
/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-eventmsg">Learn more about this API from docs.microsoft.com</see>.</para>
/// </remarks>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct EVENTMSG
{
	/// <summary>
	/// <para>Type: <b>UINT</b> The message.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-eventmsg#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint message;
	/// <summary>
	/// <para>Type: <b>UINT</b> Additional information about the message. The exact meaning depends on the <b>message</b> value.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-eventmsg#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint paramL;
	/// <summary>
	/// <para>Type: <b>UINT</b> Additional information about the message. The exact meaning depends on the <b>message</b> value.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-eventmsg#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint paramH;
	/// <summary>
	/// <para>Type: <b>DWORD</b> The time at which the message was posted.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-eventmsg#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint time;
	/// <summary>
	/// <para>Type: <b>HWND</b> A handle to the window to which the message was posted.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-eventmsg#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public IntPtr hwnd;
}
