using System.Runtime.InteropServices;

namespace ActionRepeater.Win32.Input;

/// <summary>Contains the raw input from a device.</summary>
/// <remarks>
/// <para>The handle to this structure is passed in the <i>lParam</i> parameter of <a href="https://docs.microsoft.com/windows/desktop/inputdev/wm-input">WM_INPUT</a>. To get detailed information -- such as the header and the content of the raw input -- call <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getrawinputdata">GetRawInputData</a>. To read the <b>RAWINPUT</b> in the message loop as a buffered read, call <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getrawinputbuffer">GetRawInputBuffer</a>. To get device specific information, call <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getrawinputdeviceinfoa">GetRawInputDeviceInfo</a> with the <i>hDevice</i> from <a href="https://docs.microsoft.com/windows/desktop/api/winuser/ns-winuser-rawinputheader">RAWINPUTHEADER</a>. Raw input is available only when the application calls <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-registerrawinputdevices">RegisterRawInputDevices</a> with valid device specifications.</para>
/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawinput#">Read more on docs.microsoft.com</see>.</para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct RAWINPUT
{
    public static readonly int SIZE = Marshal.SizeOf<RAWINPUT>();

    /// <summary>
    /// <para>Type: <b><a href="https://docs.microsoft.com/windows/desktop/api/winuser/ns-winuser-rawinputheader">RAWINPUTHEADER</a></b> The raw input data.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawinput#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public RAWINPUTHEADER header;
    /// <summary>
    /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-rawinput#members">see docs.microsoft.com</see>
    /// </summary>
    public RAWINPUT.UNION data;

    [StructLayout(LayoutKind.Explicit)]
    public struct UNION
    {
        [FieldOffset(0)]
        public RAWMOUSE mouse;
        [FieldOffset(0)]
        public RAWKEYBOARD keyboard;
        [FieldOffset(0)]
        public RAWHID hid;
    }
}
