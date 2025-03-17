using System;

namespace ActionRepeater.Win32.WindowsAndMessages;

// Based on https://github.com/dotnet/pinvoke (MIT)

[Flags]
public enum WindowStyle : uint
{
    /// <summary>
    /// The window has a thin-line border.
    /// </summary>
    BORDER = 0x800000,

    /// <summary>
    /// The window has a title bar (includes the BORDER style).
    /// </summary>
    CAPTION = 0xc00000,

    /// <summary>
    /// The window is a child window. A window with this style cannot have a menu bar. This style
    /// cannot be used with the POPUP style.
    /// </summary>
    CHILD = 0x40000000,

    /// <summary>
    /// Excludes the area occupied by child windows when drawing occurs within the parent window.
    /// This style is used when creating the parent window.
    /// </summary>
    CLIPCHILDREN = 0x2000000,

    /// <summary>
    /// Clips child windows relative to each other; that is, when a particular child window
    /// receives a WM_PAINT message, the CLIPSIBLINGS style clips all other overlapping child
    /// windows out of the region of the child window to be updated. If CLIPSIBLINGS is not
    /// specified and child windows overlap, it is possible, when drawing within the client area
    /// of a child window, to draw within the client area of a neighboring child window.
    /// </summary>
    CLIPSIBLINGS = 0x4000000,

    /// <summary>
    /// The window is initially disabled. A disabled window cannot receive input from the user.
    /// To change this after a window has been created, use the EnableWindow function.
    /// </summary>
    DISABLED = 0x8000000,

    /// <summary>
    /// The window has a border of a style typically used with dialog boxes. A window with this
    /// style cannot have a title bar.
    /// </summary>
    DLGFRAME = 0x400000,

    /// <summary>
    /// The window is the first control of a group of controls. The group consists of this first
    /// control and all controls defined after it, up to the next control with the GROUP
    /// style. The first control in each group usually has the TABSTOP style so that the user
    /// can move from group to group. The user can subsequently change the keyboard focus from
    /// one control in the group to the next control in the group by using the direction keys.
    /// You can turn this style on and off to change dialog box navigation. To change this style
    /// after a window has been created, use the SetWindowLong function.
    /// </summary>
    GROUP = 0x20000,

    /// <summary>
    /// The window has a horizontal scroll bar.
    /// </summary>
    HSCROLL = 0x100000,

    /// <summary>
    /// The window is initially maximized.
    /// </summary>
    MAXIMIZE = 0x1000000,

    /// <summary>
    /// The window has a maximize button. Cannot be combined with the EX_CONTEXTHELP style.
    /// The SYSMENU style must also be specified.
    /// </summary>
    MAXIMIZEBOX = 0x10000,

    /// <summary>
    /// The window is initially minimized.
    /// </summary>
    MINIMIZE = 0x20000000,

    /// <summary>
    /// The window has a minimize button. Cannot be combined with the EX_CONTEXTHELP style.
    /// The SYSMENU style must also be specified.
    /// </summary>
    MINIMIZEBOX = 0x20000,

    /// <summary>
    /// The window is an overlapped window. An overlapped window has a title bar and a border.
    /// </summary>
    OVERLAPPED = 0x0,

    /// <summary>
    /// The window is an overlapped window.
    /// </summary>
    OVERLAPPEDWINDOW = OVERLAPPED | CAPTION | SYSMENU | SIZEBOX | MINIMIZEBOX | MAXIMIZEBOX,

    /// <summary>
    /// The window is a pop-up window. This style cannot be used with the CHILD style.
    /// </summary>
    POPUP = 0x80000000u,

    /// <summary>
    /// The window is a pop-up window. The CAPTION and POPUPWINDOW styles must be combined
    /// to make the window menu visible.
    /// </summary>
    POPUPWINDOW = POPUP | BORDER | SYSMENU,

    /// <summary>
    /// The window has a sizing border.
    /// </summary>
    SIZEBOX = 0x40000,

    /// <summary>
    /// The window has a window menu on its title bar. The CAPTION style must also be specified.
    /// </summary>
    SYSMENU = 0x80000,

    /// <summary>
    /// The window is a control that can receive the keyboard focus when the user presses the TAB
    /// key. Pressing the TAB key changes the keyboard focus to the next control with the
    /// TABSTOP style. You can turn this style on and off to change dialog box navigation. To
    /// change this style after a window has been created, use the SetWindowLong function. For
    /// user-created windows and modeless dialogs to work with tab stops, alter the message loop
    /// to call the IsDialogMessage function.
    /// </summary>
    TABSTOP = 0x10000,

    /// <summary>
    /// The window is initially visible. This style can be turned on and off by using the
    /// ShowWindow or SetWindowPos function.
    /// </summary>
    VISIBLE = 0x10000000,

    /// <summary>
    /// The window has a vertical scroll bar.
    /// </summary>
    VSCROLL = 0x200000,
}
