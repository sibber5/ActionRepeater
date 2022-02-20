namespace ActionRepeater.Win32.Input;

/// <summary>Contains information about the state of the keyboard.</summary>
/// <remarks>
/// <para>For a **MakeCode** value <see href="https://docs.microsoft.com/windows-hardware/drivers/hid/keyboard-and-mouse-hid-client-drivers">HID client mapper driver</see> converts HID usages into scan codes according to <see href="https://download.microsoft.com/download/1/6/1/161ba512-40e2-4cc9-843a-923143f3456c/translate.pdf">USB HID to PS/2 Scan Code Translation Table</see> (see **PS/2 Set 1 Make** column). Older PS/2 keyboards actually transmit Scan Code Set 2 values down the wire from the keyboard to the keyboard port. These values are translated to Scan Code Set 1 by the i8042 port chip. Possible values are listed in <see href="https://download.microsoft.com/download/1/6/1/161ba512-40e2-4cc9-843a-923143f3456c/scancode.doc">Keyboard Scan Code Specification</see> (see **Scan Code Table**). <b>KEYBOARD_OVERRUN_MAKE_CODE</b> is a special **MakeCode** value sent when an invalid or unrecognizable combination of keys is pressed or the number of keys pressed exceeds the limit for this keyboard.</para>
/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawkeyboard#">Read more on docs.microsoft.com</see>.</para>
/// </remarks>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct RAWKEYBOARD
{
	/// <summary>
	/// <para>Type: <b>USHORT</b> Specifies the scan code (from Scan Code Set 1) associated with a key press. See Remarks.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawkeyboard#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public ushort MakeCode;
	/// <summary>
	/// <para>Type: <b>USHORT</b> Flags for scan code information.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawkeyboard#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public RawInputKeyFlags Flags;
	/// <summary>
	/// <para>Type: <b>USHORT</b> Reserved; must be zero.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawkeyboard#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public ushort Reserved;
	/// <summary>
	/// <para>Type: <b>USHORT</b> The corresponding [legacy virtual-key code](/windows/win32/inputdev/virtual-key-codes).</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawkeyboard#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public VirtualKey VKey;
	/// <summary>
	/// <para>Type: <b>UINT</b> The corresponding [legacy keyboard window message](/windows/win32/inputdev/keyboard-input-notifications), for example [WM_KEYDOWN](/windows/win32/inputdev/wm-keydown), [WM_SYSKEYDOWN](/windows/win32/inputdev/wm-syskeydown), and so forth.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawkeyboard#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint Message;
	/// <summary>
	/// <para>Type: <b>ULONG</b> The device-specific additional information for the event.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawkeyboard#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint ExtraInformation;
}
