using System;

namespace ActionRepeater.Win32.Input;

/// <summary>Defines information for the raw input devices.</summary>
/// <remarks>
/// <para>If <see cref="RawInputFlags.NOLEGACY"/> is set for a mouse or a keyboard, the system does not generate any legacy message for that device for the application. For example, if the mouse TLC is set with <see cref="RawInputFlags.NOLEGACY"/>, <see cref="WindowsAndMessages.WindowMessage.LBUTTONDOWN"/> and <a href="https://docs.microsoft.com/windows/win32/inputdev/mouse-input-notifications">related legacy mouse messages</a> are not generated. Likewise, if the keyboard TLC is set with <see cref="RawInputFlags.NOLEGACY"/>, <see cref="WindowsAndMessages.WindowMessage.KEYDOWN"/> and <a href="https://docs.microsoft.com/windows/win32/inputdev/keyboard-input-notifications">related legacy keyboard messages</a> are not generated.</para>
/// <para>If <see cref="RawInputFlags.REMOVE"/> is set and the <paramref name="hwndTarget"/> member is not set to <b>NULL</b> (<see cref="IntPtr.Zero"/>), then <see href="https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-registerrawinputdevices">RegisterRawInputDevices</see> function will fail.</para>
/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawinputdevice#">Read more on docs.microsoft.com</see>.</para>
/// </remarks>
public struct RAWINPUTDEVICE
{
	/// <summary>
	/// The size, in bytes, of a <see cref="RAWINPUTDEVICE"/> structure.
	/// </summary>
	public static readonly int SIZE = System.Runtime.InteropServices.Marshal.SizeOf(typeof(RAWINPUTDEVICE));

	/// <summary>
	/// <see href="https://docs.microsoft.com/windows-hardware/drivers/hid/top-level-collections">Top level collection</see> <see href="https://docs.microsoft.com/windows-hardware/drivers/hid/hid-usages#usage-page">Usage page</see> for the raw input device. See <see href="https://docs.microsoft.com/windows-hardware/drivers/hid/hid-architecture#hid-clients-supported-in-windows">HID Clients Supported in Windows</see> for details on possible values.
	/// </summary>
	public UsagePage usUsagePage;
	/// <summary>
	/// <see href="https://docs.microsoft.com/windows-hardware/drivers/hid/top-level-collections">Top level collection</see> <see href="https://docs.microsoft.com/en-us/windows-hardware/drivers/hid/hid-usages#usage-id">Usage ID</see> for the raw input device. See <see href="https://docs.microsoft.com/en-us/windows-hardware/drivers/hid/hid-architecture#hid-clients-supported-in-windows">HID Clients Supported in Windows</see> for details on possible values.
	/// </summary>
	public UsageId usUsage;
	/// <summary>
	/// Mode flag that specifies how to interpret the information provided by <paramref name="usUsagePage"/> and <paramref name="usUsage"/>. By default, the operating system sends raw input from devices with the specified <see href="https://docs.microsoft.com/windows-hardware/drivers/hid/top-level-collections">top level collection</see> (TLC) to the registered application as long as it has the window focus.
	/// </summary>
	public RawInputFlags dwFlags;
	/// <summary>
	/// <para>Type: <b>HWND</b> A handle to the target window. If <b>NULL</b> (<see cref="IntPtr.Zero"/>) it follows the keyboard focus.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawinputdevice#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public IntPtr hwndTarget;
}
