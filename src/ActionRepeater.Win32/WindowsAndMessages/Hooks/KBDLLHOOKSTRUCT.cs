using ActionRepeater.Win32.Input;

namespace ActionRepeater.Win32.WindowsAndMessages;

/// <summary>Contains information about a low-level keyboard input event.</summary>
/// <remarks>
/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-kbdllhookstruct">Learn more about this API from docs.microsoft.com</see>.</para>
/// </remarks>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct KBDLLHOOKSTRUCT
{
	/// <summary>
	/// <para>Type: <b>DWORD</b> A <a href="https://docs.microsoft.com/windows/desktop/inputdev/virtual-key-codes">virtual-key code</a>. The code must be a value in the range 1 to 254.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-kbdllhookstruct#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public VirtualKey vkCode;
	/// <summary>
	/// <para>Type: <b>DWORD</b> A hardware scan code for the key.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-kbdllhookstruct#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public ScanCode scanCode;
	/// <summary>
	/// <para>Type: <b>DWORD</b> The extended-key flag, event-injected flags, context code, and transition-state flag. This member is specified as follows. An application can use the following values to test the keystroke flags. Testing LLKHF_INJECTED (bit 4) will tell you whether the event was injected. If it was, then testing LLKHF_LOWER_IL_INJECTED (bit 1) will tell you whether or not the event was injected from a process running at lower integrity level. </para>
	/// <para>This doc was truncated.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-kbdllhookstruct#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint flags;
	/// <summary>
	/// <para>Type: <b>DWORD</b> The time stamp for this message, equivalent to what <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getmessagetime">GetMessageTime</a> would return for this message.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-kbdllhookstruct#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint time;
	/// <summary>
	/// <para>Type: <b>ULONG_PTR</b> Additional information associated with the message.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-kbdllhookstruct#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public nuint dwExtraInfo;
}
