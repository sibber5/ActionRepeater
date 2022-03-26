using System;

namespace ActionRepeater.Win32.Input;

/// <summary>
/// Specifies various aspects of a keystroke. This member can be certain combinations of the following values.
/// </summary>
/// <remarks>If <see cref="KEYUP"/> is not specified, the key is being pressed.</remarks>
[Flags]
public enum KEYEVENTF : uint
{
	/// <summary>
	/// If specified, the scan code was preceded by a prefix byte that has the value 0xE0 (224). 
	/// </summary>
	EXTENDEDKEY = 0x00000001,
	/// <summary>
	/// If specified, the key is being released. If not specified, the key is being pressed.
	/// </summary>
	KEYUP = 0x00000002,
	/// <summary>
	/// If specified, wScan identifies the key and wVk is ignored. 
	/// </summary>
	SCANCODE = 0x00000008,
	/// <summary>
	/// <para>If specified, the system synthesizes a VK_PACKET keystroke. The wVk parameter must be zero. This flag can only be combined with the KEYEVENTF_KEYUP flag.</para>
	/// <para><see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-keybdinput#remarks">For more information, see docs.microsoft.com</see></para>
	/// </summary>
	UNICODE = 0x00000004,
}
