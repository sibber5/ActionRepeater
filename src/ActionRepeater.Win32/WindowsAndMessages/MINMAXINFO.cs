namespace ActionRepeater.Win32.WindowsAndMessages;

/// <summary>Contains information about a window's maximized size and position and its minimum and maximum tracking size.</summary>
/// <remarks>
/// <para>For systems with multiple monitors, the <b>ptMaxSize</b> and <b>ptMaxPosition</b> members describe the maximized size and position of the window on the primary monitor, even if the window ultimately maximizes onto a secondary monitor. In that case, the window manager adjusts these values to compensate for differences between the primary monitor and the monitor that displays the window. Thus, if the user leaves <b>ptMaxSize</b> untouched, a window on a monitor larger than the primary monitor maximizes to the size of the larger monitor.</para>
/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-minmaxinfo#">Read more on docs.microsoft.com</see>.</para>
/// </remarks>
public struct MINMAXINFO
{
    /// <summary>
    /// <para>Type: <b><a href="https://docs.microsoft.com/previous-versions/dd162805(v=vs.85)">POINT</a></b> Reserved; do not use.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-minmaxinfo#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public POINT ptReserved;
    /// <summary>
    /// <para>Type: <b><a href="https://docs.microsoft.com/previous-versions/dd162805(v=vs.85)">POINT</a></b> The maximized width (<b>x</b> member) and the maximized height (<b>y</b> member) of the window. For top-level windows, this value is based on the width of the primary monitor.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-minmaxinfo#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public POINT ptMaxSize;
    /// <summary>
    /// <para>Type: <b><a href="https://docs.microsoft.com/previous-versions/dd162805(v=vs.85)">POINT</a></b> The position of the left side of the maximized window (<b>x</b> member) and the position of the top of the maximized window (<b>y</b> member). For top-level windows, this value is based on the position of the primary monitor.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-minmaxinfo#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public POINT ptMaxPosition;
    /// <summary>
    /// <para>Type: <b><a href="https://docs.microsoft.com/previous-versions/dd162805(v=vs.85)">POINT</a></b> The minimum tracking width (<b>x</b> member) and the minimum tracking height (<b>y</b> member) of the window. This value can be obtained programmatically from the system metrics <b>SM_CXMINTRACK</b> and <b>SM_CYMINTRACK</b> (see the <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getsystemmetrics">GetSystemMetrics</a> function).</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-minmaxinfo#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public POINT ptMinTrackSize;
    /// <summary>
    /// <para>Type: <b><a href="https://docs.microsoft.com/previous-versions/dd162805(v=vs.85)">POINT</a></b> The maximum tracking width (<b>x</b> member) and the maximum tracking height (<b>y</b> member) of the window. This value is based on the size of the virtual screen and can be obtained programmatically from the system metrics <b>SM_CXMAXTRACK</b> and <b>SM_CYMAXTRACK</b> (see the <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getsystemmetrics">GetSystemMetrics</a> function).</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-minmaxinfo#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public POINT ptMaxTrackSize;
}
