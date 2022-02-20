namespace ActionRepeater.Win32.Input;

#pragma warning disable CA1069, RCS1234, RCS1191

/// <summary>
/// Flags used by <see cref="RAWINPUTDEVICE"/>.
/// </summary>
[Flags]
public enum RawInputFlags : uint
{
	ZERO = 0u,
	/// <summary>
	/// If set, this removes the top level collection from the inclusion list. This tells the operating system to stop reading from a device which matches the top level collection.
	/// </summary>
	REMOVE = 0x00000001u,
	/// <summary>
	/// If set, this specifies the top level collections to exclude when reading a complete usage page. This flag only affects a TLC whose usage page is already specified with <see cref="PAGEONLY"/>.
	/// </summary>
	EXCLUDE = 0x00000010u,
	/// <summary>
	/// If set, this specifies all devices whose top level collection is from the specified <paramref name="usUsagePage"/>. Note that <paramref name="usUsage"/> must be zero. To exclude a particular top level collection, use <see cref="EXCLUDE"/>.
	/// </summary>
	PAGEONLY = 0x00000020u,
	/// <summary>
	/// If set, this prevents any devices specified by <paramref name="usUsagePage"/> or usUsage from generating <see href="https://docs.microsoft.com/en-us/windows/win32/inputdev/mouse-input-notifications">legacy messages</see>. This is only for the mouse and keyboard. See <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-rawinputdevice#remarks">Remarks</see>.
	/// </summary>
	NOLEGACY = 0x00000030u,
	/// <summary>
	/// If set, this enables the caller to receive the input even when the caller is not in the foreground. Note that <paramref name="hwndTarget"/> must be specified.
	/// </summary>
	INPUTSINK = 0x00000100u,
	/// <summary>
	/// If set, the mouse button click does not activate the other window. <see cref="CAPTUREMOUSE"/> can be specified only if <see cref="NOLEGACY"/> is specified for a mouse device.
	/// </summary>
	CAPTUREMOUSE = 0x00000200u,
	/// <summary>
	/// If set, the application-defined keyboard device hotkeys are not handled. However, the system hotkeys; for example, ALT+TAB and CTRL+ALT+DEL, are still handled. By default, all keyboard hotkeys are handled. <see cref="NOHOTKEYS"/> can be specified even if <see cref="NOLEGACY"/> is not specified and <paramref name="hwndTarget"/> is <see cref="IntPtr.Zero"/>.
	/// </summary>
	NOHOTKEYS = 0x00000200u,
	/// <summary>
	/// If set, the application command keys are handled. <see cref="APPKEYS"/> can be specified only if <see cref="NOLEGACY"/> is specified for a keyboard device.
	/// </summary>
	APPKEYS = 0x00000400u,
	/// <summary>
	/// If set, this enables the caller to receive input in the background only if the foreground application does not process it. In other words, if the foreground application is not registered for raw input, then the background application that is registered will receive the input.<br/>
	/// <b>Windows XP:</b> This flag is not supported until Windows Vista
	/// </summary>
	EXINPUTSINK = 0x00001000u,
	/// <summary>
	/// If set, this enables the caller to receive <see cref="WindowsAndMessages.WindowMessage.INPUT_DEVICE_CHANGE"/> notifications for device arrival and device removal.<br/>
	/// <b>Windows XP:</b> This flag is not supported until Windows Vista
	/// </summary>
	DEVNOTIFY = 0x00002000u,
}
