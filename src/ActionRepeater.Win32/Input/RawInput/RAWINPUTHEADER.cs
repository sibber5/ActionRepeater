using System;
using System.Runtime.InteropServices;

namespace ActionRepeater.Win32.Input;

/// <summary>Contains the header information that is part of the raw input data.</summary>
/// <remarks>
/// <para>To get more information on the device, use <b>hDevice</b> in a call to <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getrawinputdeviceinfoa">GetRawInputDeviceInfo</see>.</para>
/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawinputheader#">Read more on docs.microsoft.com</see>.</para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct RAWINPUTHEADER
{
    public static readonly int SIZE = Marshal.SizeOf<RAWINPUTHEADER>();

    /// <summary>
    /// Type: <b>DWORD</b> The type of raw input.
    /// </summary>
    public RawInputType dwType;
    /// <summary>
    /// <para>Type: <b>DWORD</b> The size, in bytes, of the entire input packet of data. This includes <see cref="RAWINPUT"/> plus possible extra input reports in the <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-rawhid">RAWHIDM</see> variable length array.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawinputheader#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public uint dwSize;
    /// <summary>
    /// <para>Type: <b>HANDLE</b> A handle to the device generating the raw input data.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawinputheader#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public IntPtr hDevice;
    /// <summary>
    /// <para>Type: <b>WPARAM</b> The value passed in the <i>wParam</i> parameter of the <see cref="WindowsAndMessages.WindowMessage.INPUT"/> message.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawinputheader#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public nuint wParam;
}
