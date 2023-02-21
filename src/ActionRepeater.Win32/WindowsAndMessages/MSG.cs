using System;
using System.Runtime.InteropServices;

namespace ActionRepeater.Win32.WindowsAndMessages;

[StructLayout(LayoutKind.Sequential)]
public struct MSG
{
    /// <summary>
    /// <para>Type: <b>HWND</b> A handle to the window whose window procedure receives the message. This member is <b>NULL</b> when the message is a thread message.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msg#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public nint hwnd;
    /// <summary>
    /// <para>Type: <b>UINT</b> The message identifier. Applications can only use the low word; the high word is reserved by the system.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msg#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public uint message;
    /// <summary>
    /// <para>Type: <b>WPARAM</b> Additional information about the message. The exact meaning depends on the value of the <b>message</b> member.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msg#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public nuint wParam;
    /// <summary>
    /// <para>Type: <b>LPARAM</b> Additional information about the message. The exact meaning depends on the value of the <b>message</b> member.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msg#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public nint lParam;
    /// <summary>
    /// <para>Type: <b>DWORD</b> The time at which the message was posted.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msg#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public uint time;
    /// <summary>
    /// <para>Type: <b><a href="https://docs.microsoft.com/previous-versions/dd162805(v=vs.85)">POINT</a></b> The cursor position, in screen coordinates, when the message was posted.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msg#members">Read more on docs.microsoft.com</see>.</para>
    /// </summary>
    public POINT pt;

    public override string ToString()
    {
        return $"{(WindowMessage)message}: lParam={lParam} wParam={wParam}";
    }
}
