namespace ActionRepeater.Win32;

/// <summary>The RECT structure defines a rectangle by the coordinates of its upper-left and lower-right corners.</summary>
/// <remarks>
/// <para>The RECT structure is identical to the <a href="https://docs.microsoft.com/windows/desktop/api/windef/ns-windef-rectl">RECTL</a> structure.</para>
/// <para><see href="https://docs.microsoft.com/windows/win32/api//windef/ns-windef-rect#">Read more on docs.microsoft.com</see>.</para>
/// </remarks>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct RECT
{
	/// <summary>Specifies the <i>x</i>-coordinate of the upper-left corner of the rectangle.</summary>
	public int left;
	/// <summary>Specifies the <i>y</i>-coordinate of the upper-left corner of the rectangle.</summary>
	public int top;
	/// <summary>Specifies the <i>x</i>-coordinate of the lower-right corner of the rectangle.</summary>
	public int right;
	/// <summary>Specifies the <i>y</i>-coordinate of the lower-right corner of the rectangle.</summary>
	public int bottom;
}
