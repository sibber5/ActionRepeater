using System.Runtime.InteropServices;

namespace ActionRepeater.Win32.Input;

/// <summary>Used by <see cref="PInvoke.SendInput"/> to store information for synthesizing input events such as keystrokes, mouse movement, and mouse clicks.</summary>
/// <remarks>
/// <para><b> INPUT_KEYBOARD</b> supports nonkeyboard input methods, such as handwriting recognition or voice recognition, as if it were text input by using the <b>KEYEVENTF_UNICODE</b> flag. For more information, see the remarks section of <a href="https://docs.microsoft.com/windows/desktop/api/winuser/ns-winuser-keybdinput">KEYBDINPUT</a>.</para>
/// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/ns-winuser-input">Read more on docs.microsoft.com</see>.</para>
/// </remarks>
public struct INPUT
{
	/// <summary>
	/// The size, in bytes, of an <see cref="INPUT"/> struct.
	/// </summary>
	public static readonly int SIZE = Marshal.SizeOf<INPUT>();

	/// <summary>Type: <b>DWORD</b></summary>
	public INPUT.TYPE type;
	public INPUT.UNION union;

	[StructLayout(LayoutKind.Explicit)]
	public struct UNION
	{
		[FieldOffset(0)]
		public MOUSEINPUT mi;
		[FieldOffset(0)]
		public KEYBDINPUT ki;
		[FieldOffset(0)]
		public HARDWAREINPUT hi;
	}

	public enum TYPE : uint
    {
		MOUSE = 0u,
		KEYBOARD = 1u,
		HARDWARE = 2u,
	}
}
