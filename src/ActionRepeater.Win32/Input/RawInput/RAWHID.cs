namespace ActionRepeater.Win32.Input;

/// <summary>Describes the format of the raw input from a Human Interface Device (HID).</summary>
/// <remarks>
/// <para>Each <a href="https://docs.microsoft.com/windows/desktop/inputdev/wm-input">WM_INPUT</a> can indicate several inputs, but all of the inputs come from the same HID. The size of the <b>bRawData</b> array is <b>dwSizeHid</b> *	<b>dwCount</b>. For more information, see <a href="https://docs.microsoft.com/windows-hardware/drivers/hid/interpreting-hid-reports">Interpreting HID Reports</a>.</para>
/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawhid#">Read more on docs.microsoft.com</see>.</para>
/// </remarks>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct RAWHID
{
	/// <summary>
	/// <para>Type: <b>DWORD</b> The size, in bytes, of each HID input in <b>bRawData</b>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawhid#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint dwSizeHid;
	/// <summary>
	/// <para>Type: <b>DWORD</b> The number of HID inputs in <b>bRawData</b>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawhid#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint dwCount;
	/// <summary>
	/// <para>Type: <b>BYTE[1]</b> The raw input data, as an array of bytes.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawhid#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public byte bRawData;
}
