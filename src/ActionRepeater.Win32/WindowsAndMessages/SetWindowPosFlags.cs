namespace ActionRepeater.Win32.WindowsAndMessages;

[Flags]
public enum SetWindowPosFlags : uint
{
	/// <summary>
	/// If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request. 
	/// </summary>
	ASYNCWINDOWPOS = 0x4000u,
	/// <summary>
	/// Prevents generation of the <b>WM_SYNCPAINT</b> message. 
	/// </summary>
	DEFERERASE = 0x2000u,
	/// <summary>
	/// Draws a frame (defined in the window's class description) around the window. 
	/// </summary>
	DRAWFRAME = 0x0020u,
	/// <summary>
	/// Applies new frame styles set using the <b>SetWindowLong</b> function. Sends a <b>WM_NCCALCSIZE</b> message to the window, even if the window's size is not being changed. If this flag is not specified, <b>WM_NCCALCSIZE</b> is sent only when the window's size is being changed. 
	/// </summary>
	FRAMECHANGED = 0x0020u,
	/// <summary>
	/// Hides the window. 
	/// </summary>
	HIDEWINDOW = 0x0080u,
	/// <summary>
	/// Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the <paramref name="hWndInsertAfter"/> parameter).
	/// </summary>
	NOACTIVATE = 0x0010u,
	/// <summary>
	/// Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned. 
	/// </summary>
	NOCOPYBITS = 0x0100u,
	/// <summary>
	/// Retains the current position (ignores <i>X</i> and <i>Y</i> parameters). 
	/// </summary>
	NOMOVE = 0x0002u,
	/// <summary>
	/// Retains the current position (ignores X and Y parameters). 
	/// </summary>
	NOOWNERZORDER = 0x0200u,
	/// <summary>
	/// Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing. 
	/// </summary>
	NOREDRAW = 0x0008u,
	/// <summary>
	/// Same as the <see cref="NOOWNERZORDER"/> flag.
	/// </summary>
	NOREPOSITION = 0x0200u,
	/// <summary>
	/// Prevents the window from receiving the <b>WM_WINDOWPOSCHANGING</b> message.
	/// </summary>
	NOSENDCHANGING = 0x0400u,
	/// <summary>
	/// Retains the current size (ignores the cx and cy parameters). 
	/// </summary>
	NOSIZE = 0x0001u,
	/// <summary>
	/// Retains the current Z order (ignores the <paramref name="hWndInsertAfter"/> parameter).
	/// </summary>
	NOZORDER = 0x0004u,
	/// <summary>
	/// Displays the window. 
	/// </summary>
	SHOWWINDOW = 0x0040u,
}
