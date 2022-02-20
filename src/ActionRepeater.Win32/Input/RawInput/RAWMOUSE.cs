using System.Runtime.InteropServices;

namespace ActionRepeater.Win32.Input;

/// <summary>Contains information about the state of the mouse.</summary>
/// <remarks>
/// <para>If the mouse has moved, indicated by MOUSE_MOVE_RELATIVE or MOUSE_MOVE_ABSOLUTE, lLastX and lLastY specify information about that movement. The information is specified as relative or absolute integer values.</para>
/// <para>If MOUSE_MOVE_RELATIVE value is specified, lLastX and lLastY specify movement relative to the previous mouse event (the last reported position). Positive values mean the mouse moved right (or down); negative values mean the mouse moved left (or up).</para>
/// <para>If MOUSE_MOVE_ABSOLUTE value is specified, lLastX and lLastY contain normalized absolute coordinates between 0 and 65,535. Coordinate (0,0) maps onto the upper-left corner of the display surface; coordinate (65535,65535) maps onto the lower-right corner. In a multimonitor system, the coordinates map to the primary monitor.</para>
/// <para>If MOUSE_VIRTUAL_DESKTOP is specified in addition to MOUSE_MOVE_ABSOLUTE, the coordinates map to the entire virtual desktop.</para>
/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawmouse#">Read more on docs.microsoft.com</see>.</para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct RAWMOUSE
{
	/// <summary>
	/// <para>Type: **USHORT** The mouse state.
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawmouse#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public RawMouseState usFlags;

	public RawButtonData rawButtonData;

	/// <summary>
	/// <para>Type: **ULONG** The raw state of the mouse buttons. The Win32 subsystem does not use this member.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawmouse#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint ulRawButtons;

	/// <summary>
	/// <para>Type: **LONG** The motion in the X direction. This is signed relative motion or absolute motion, depending on the value of <see cref="usFlags"/>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawmouse#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public int lLastX;

	/// <summary>
	/// <para>Type: **LONG** The motion in the Y direction. This is signed relative motion or absolute motion, depending on the value of <see cref="usFlags"/>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawmouse#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public int lLastY;

	/// <summary>
	/// <para>Type: **ULONG** The device-specific additional information for the event.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawmouse#members">Read more on docs.microsoft.com</see>.</para>
	/// </summary>
	public uint ulExtraInformation;

    [StructLayout(LayoutKind.Explicit)]
    public struct RawButtonData
    {
        /// <summary>
        /// Reserved.
        /// </summary>
        [FieldOffset(0)]
        public uint ulButtons;
        [FieldOffset(0)]
        public RawButtonInfo buttonInfo;

        [StructLayout(LayoutKind.Sequential)]
        public struct RawButtonInfo
        {
            /// <summary>
            /// The transition state of the mouse buttons.
            /// </summary>
            public RawMouseButtonState usButtonFlags;
            /// <summary>
            /// <para>If <see cref="usButtonFlags"/> has <see cref="RawMouseButtonState.WHEEL"/> or <see cref="RawMouseButtonState.HWHEEL"/>, this member specifies the distance the wheel is rotated.</para>
            /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-rawmouse#remarks">Read more on docs.microsoft.com</see>.</para>
            /// </summary>
            public ushort usButtonData;
        }
    }

    //[StructLayout(LayoutKind.Explicit)]
    //public struct Data
    //{
    //	[FieldOffset(0)]
    //	public uint ulButtons;
    //	/// <summary>
    //	/// If the mouse wheel is moved, this will contain the delta amount.
    //	/// </summary>
    //	[FieldOffset(2)]
    //	public ushort usButtonData;
    //	/// <summary>
    //	/// Flags for the event.
    //	/// </summary>
    //	[FieldOffset(0)]
    //	public ushort usButtonFlags;
    //}
}
