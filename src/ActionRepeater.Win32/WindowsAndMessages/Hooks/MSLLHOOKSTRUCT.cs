namespace ActionRepeater.Win32.WindowsAndMessages;

/// <summary>Contains information about a low-level mouse input event.</summary>
/// <remarks>
/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msllhookstruct">Learn more about this API from docs.microsoft.com</see>.</para>
/// </remarks>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct MSLLHOOKSTRUCT
{
	/// <summary>
	/// <para>Type: <b><a href="https://docs.microsoft.com/previous-versions/dd162805(v=vs.85)">POINT</a></b> The x- and y-coordinates of the cursor, in <a href="https://docs.microsoft.com/windows/desktop/api/shellscalingapi/ne-shellscalingapi-process_dpi_awareness">per-monitor-aware</a> screen coordinates.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msllhookstruct#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public POINT pt;
	/// <summary>
	/// <para>Type: <b>DWORD</b> If the message is <a href="https://docs.microsoft.com/windows/desktop/inputdev/wm-mousewheel">WM_MOUSEWHEEL</a>, the high-order word of this member is the wheel delta. The low-order word is reserved. A positive value indicates that the wheel was rotated forward, away from the user; a negative value indicates that the wheel was rotated backward, toward the user. One wheel click is defined as <b>WHEEL_DELTA</b>, which is 120.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msllhookstruct#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint mouseData;
	/// <summary>
	/// <para>Type: <b>DWORD</b> The event-injected flags. An application can use the following values to test the flags. Testing LLMHF_INJECTED (bit 0) will tell you whether the event was injected. If it was, then testing LLMHF_LOWER_IL_INJECTED (bit 1) will tell you whether or not the event was injected from a process running at lower integrity level. </para>
	/// <para>This doc was truncated.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msllhookstruct#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint flags;
	/// <summary>
	/// <para>Type: <b>DWORD</b> The time stamp for this message.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msllhookstruct#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint time;
	/// <summary>
	/// <para>Type: <b>ULONG_PTR</b> Additional information associated with the message.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msllhookstruct#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public nuint dwExtraInfo;
}
