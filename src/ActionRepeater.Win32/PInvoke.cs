using System.Runtime.InteropServices;
using SupportedOSPlatformAttribute = System.Runtime.Versioning.SupportedOSPlatformAttribute;
using ActionRepeater.Win32.Input;
using ActionRepeater.Win32.WindowsAndMessages;

#pragma warning disable CA1401 // P/Invokes should not be visible
[assembly: SupportedOSPlatform("windows5.1.2600")]
namespace ActionRepeater.Win32;

public static partial class PInvoke
{
    /// <inheritdoc cref="SendInput(uint, INPUT*, int)"/>
    public static unsafe uint SendInput(Span<INPUT> pInputs)
	{
		fixed (INPUT* pInputsLocal = pInputs)
		{
			uint __result = SendInput((uint)pInputs.Length, pInputsLocal, INPUT.SIZE);
			return __result;
		}
	}

	/// <summary>Synthesizes keystrokes, mouse motions, and button clicks.</summary>
	/// <param name="cInputs">
	/// <para>Type: <b>UINT</b> The number of structures in the <i>pInputs</i> array.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-sendinput#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="pInputs">
	/// <para>Type: <b>LPINPUT</b> An array of <a href="https://docs.microsoft.com/windows/desktop/api/winuser/ns-winuser-input">INPUT</a> structures. Each structure represents an event to be inserted into the keyboard or mouse input stream.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-sendinput#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="cbSize">
	/// <para>Type: <b>int</b> The size, in bytes, of an <a href="https://docs.microsoft.com/windows/desktop/api/winuser/ns-winuser-input">INPUT</a> structure. If <i>cbSize</i> is not the size of an <b>INPUT</b> structure, the function fails.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-sendinput#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>UINT</b> The function returns the number of events that it successfully inserted into the keyboard or mouse input stream. If the function returns zero, the input was already blocked by another thread. To get extended error information, call <see cref="Marshal.GetLastPInvokeError"/>. This function fails when it is blocked by UIPI. Note that neither <see cref="Marshal.GetLastPInvokeError"/> nor the return value will indicate the failure was caused by UIPI blocking.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-sendinput">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("User32", ExactSpelling = true, SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern unsafe uint SendInput(uint cInputs, INPUT* pInputs, int cbSize);

	/// <summary>Translates (maps) a virtual-key code into a scan code or character value, or translates a scan code into a virtual-key code.</summary>
	/// <param name="uCode">
	/// <para>Type: <b>UINT</b> The <see href="https://docs.microsoft.com/windows/desktop/inputdev/virtual-key-codes">virtual key code</see> or scan code for a key. How this value is interpreted depends on the value of the <paramref name="uMapType"/> parameter. <b>Starting with Windows Vista</b>, the high byte of the <paramref name="uCode"/> value can contain either <i>0xE0</i> or <i>0xE1</i> to specify the extended scan code.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-mapvirtualkeyw#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>UINT</b> The return value is either a scan code, a virtual-key code, or a character value, depending on the value of <paramref name="uCode"/> and <paramref name="uMapType"/>. If there is no translation, the return value is <i>0</i> (zero).</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-mapvirtualkeyw">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("User32", ExactSpelling = true, EntryPoint = "MapVirtualKeyW")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern uint MapVirtualKey(uint uCode, VirtualKeyMapType uMapType);

	/// <inheritdoc cref="GetCursorPos(POINT*)"/>
	public static unsafe bool GetCursorPos(out POINT lpPoint)
	{
		fixed (POINT* lpPointLocal = &lpPoint)
		{
			bool __result = GetCursorPos(lpPointLocal);
			return __result;
		}
	}

	/// <summary>Retrieves the position of the mouse cursor, in screen coordinates.</summary>
	/// <param name="lpPoint">
	/// <para>Type: <b>LPPOINT</b> A pointer to a <see cref="POINT"/> structure that receives the screen coordinates of the cursor.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getcursorpos#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>BOOL</b> Returns nonzero if successful or zero otherwise. To get extended error information, call <see cref="Marshal.GetLastPInvokeError"/>.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getcursorpos">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("User32", ExactSpelling = true, SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern unsafe BOOL GetCursorPos(POINT* lpPoint);

	/// <summary>Returns the dots per inch (dpi) value for the associated window.</summary>
	/// <param name="hwnd">The window you want to get information about.</param>
	/// <returns>The DPI for the window which depends on the <a href="https://docs.microsoft.com/windows/desktop/api/windef/ne-windef-dpi_awareness">DPI_AWARENESS</a> of the window. See the Remarks for more information. An invalid <i>hwnd</i> value will result in a return value of 0.</returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-getdpiforwindow">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("User32", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	[SupportedOSPlatform("windows10.0.14393")]
	public static extern uint GetDpiForWindow(IntPtr hwnd);

	/// <summary>Changes the size, position, and Z order of a child, pop-up, or top-level window. These windows are ordered according to their appearance on the screen. The topmost window receives the highest rank and is the first window in the Z order.</summary>
	/// <param name="hWnd">
	/// <para>A handle to the window.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setwindowpos#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="hWndInsertAfter">
	/// A handle to the window to precede the positioned window in the Z order. This parameter must be a window handle or one of <see cref="SpecialWindowHandles"/>'s values.
	/// </param>
	/// <param name="X">
	/// <para>Type: <b>int</b> The new position of the left side of the window, in client coordinates.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setwindowpos#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="Y">
	/// <para>Type: <b>int</b> The new position of the top of the window, in client coordinates.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setwindowpos#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="cx">
	/// <para>Type: <b>int</b> The new width of the window, in pixels.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setwindowpos#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="cy">
	/// <para>Type: <b>int</b> The new height of the window, in pixels.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setwindowpos#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="uFlags">Type: <b>UINT</b> The window sizing and positioning flags.</param>
	/// <returns>
	/// <para>Type: <b>BOOL</b> If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call <see cref="Marshal.GetLastPInvokeError"/>.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setwindowpos">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("User32", ExactSpelling = true, SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern BOOL SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

	/// <summary>Installs an application-defined hook procedure into a hook chain.</summary>
	/// <param name="idHook">
	/// Type: <b>int</b> The type of hook procedure to be installed.
	/// </param>
	/// <param name="lpfn">
	/// <para>Type: <b>HOOKPROC</b> A pointer to the hook procedure. If the <paramref name="dwThreadId"/> parameter is zero or specifies the identifier of a thread created by a different process, the <paramref name="lpfn"/> parameter must point to a hook procedure in a DLL. Otherwise, <paramref name="lpfn"/> can point to a hook procedure in the code associated with the current process.</para>
	/// <para>You can pass any delegate that is identical to <see cref="HOOKPROC"/>, <c>e.g. `new HOOKPROC(yourDelegate)`</c>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setwindowshookexw#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="hmod">
	/// <para>Type: <b>HINSTANCE</b> A handle to the DLL containing the hook procedure pointed to by the <paramref name="lpfn"/> parameter. The <paramref name="hmod"/> parameter must be set to <typeparamref name="null"/> if the <paramref name="dwThreadId"/> parameter specifies a thread created by the current process and if the hook procedure is within the code associated with the current process.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setwindowshookexw#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="dwThreadId">
	/// <para>Type: <b>DWORD</b> The identifier of the thread with which the hook procedure is to be associated. For desktop apps, if this parameter is zero, the hook procedure is associated with all existing threads running in the same desktop as the calling thread. For Windows Store apps, see the Remarks section.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setwindowshookexw#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>If the function succeeds, the return value is the handle to the hook procedure.<br/>
	/// If the function fails, the return value is an invalid handle. To get extended error information, call <see cref="Marshal.GetLastPInvokeError"/>.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setwindowshookexw">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	public static unsafe SafeHookHandle SetWindowsHookEx(WindowsHookType idHook, HOOKPROC lpfn, SafeHandle? hmod, uint dwThreadId)
	{
		bool hmodAddRef = false;
		try
		{
			IntPtr hmodLocal = IntPtr.Zero;
			if (hmod is not null)
			{
				hmod.DangerousAddRef(ref hmodAddRef);
				hmodLocal = hmod.DangerousGetHandle();
			}
			IntPtr __result = SetWindowsHookEx(idHook, lpfn, hmodLocal, dwThreadId);
			return new SafeHookHandle(__result, ownsHandle: true);
		}
		finally
		{
			if (hmodAddRef)
			{
                hmod?.DangerousRelease();
			}
		}
	}

	/// <summary>Installs an application-defined hook procedure into a hook chain.</summary>
	/// <param name="idHook">
	/// Type: <b>int</b> The type of hook procedure to be installed.
	/// </param>
	/// <param name="lpfn">
	/// <para>Type: <b>HOOKPROC</b> A pointer to the hook procedure. If the <paramref name="dwThreadId"/> parameter is zero or specifies the identifier of a thread created by a different process, the <paramref name="lpfn"/> parameter must point to a hook procedure in a DLL. Otherwise, <paramref name="lpfn"/> can point to a hook procedure in the code associated with the current process.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setwindowshookexw#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="hmod">
	/// <para>Type: <b>HINSTANCE</b> A handle to the DLL containing the hook procedure pointed to by the <paramref name="lpfn"/> parameter. The <paramref name="hmod"/> parameter must be set to <b>NULL</b> (<see cref="IntPtr.Zero"/>) if the <paramref name="dwThreadId"/> parameter specifies a thread created by the current process and if the hook procedure is within the code associated with the current process.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setwindowshookexw#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="dwThreadId">
	/// <para>Type: <b>DWORD</b> The identifier of the thread with which the hook procedure is to be associated. For desktop apps, if this parameter is zero, the hook procedure is associated with all existing threads running in the same desktop as the calling thread. For Windows Store apps, see the Remarks section.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setwindowshookexw#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>HHOOK</b> If the function succeeds, the return value is the handle to the hook procedure. If the function fails, the return value is <b>NULL</b> (<see cref="IntPtr.Zero"/>). To get extended error information, call <see cref="Marshal.GetLastPInvokeError"/>.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setwindowshookexw">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("User32", ExactSpelling = true, EntryPoint = "SetWindowsHookExW", SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern IntPtr SetWindowsHookEx(WindowsHookType idHook, HOOKPROC lpfn, IntPtr hmod, uint dwThreadId);

	/// <summary>Removes a hook procedure installed in a hook chain by the <seealso cref="SetWindowsHookEx">SetWindowsHookEx</seealso> function.</summary>
	/// <param name="hhk">
	/// <para>Type: <b>HHOOK</b> A handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to <seealso cref="SetWindowsHookEx(WindowsHookType, HOOKPROC, IntPtr, uint)">SetWindowsHookEx</seealso>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-unhookwindowshookex#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>BOOL</b> If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call <see cref="Marshal.GetLastPInvokeError"/>.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-unhookwindowshookex">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("User32", ExactSpelling = true, SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern BOOL UnhookWindowsHookEx(IntPtr hhk);

	/// <inheritdoc cref="CallNextHookEx(IntPtr, int, nuint, nint)"/>
	public static nint CallNextHookEx(int nCode, nuint wParam, nint lParam) => CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);

	/// <summary>Passes the hook information to the next hook procedure in the current hook chain. A hook procedure can call this function either before or after processing the hook information.</summary>
	/// <param name="hhk">
	/// <para>Type: <b>HHOOK</b> This parameter is ignored.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-callnexthookex#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="nCode">
	/// <para>Type: <b>int</b> The hook code passed to the current hook procedure. The next hook procedure uses this code to determine how to process the hook information.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-callnexthookex#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="wParam">
	/// <para>Type: <b>WPARAM</b> The <paramref name="wParam"/> value passed to the current hook procedure. The meaning of this parameter depends on the type of hook associated with the current hook chain.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-callnexthookex#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="lParam">
	/// <para>Type: <b>LPARAM</b> The <paramref name="lParam"/> value passed to the current hook procedure. The meaning of this parameter depends on the type of hook associated with the current hook chain.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-callnexthookex#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>LRESULT</b> This value is returned by the next hook procedure in the chain. The current hook procedure must also return this value. The meaning of the return value depends on the hook type. For more information, see the descriptions of the individual hook procedures.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-callnexthookex">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("User32", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern nint CallNextHookEx(IntPtr hhk, int nCode, nuint wParam, nint lParam);

	/// <summary>Retrieves the thread identifier of the calling thread.</summary>
	/// <returns>The return value is the thread identifier of the calling thread.</returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//processthreadsapi/nf-processthreadsapi-getcurrentthreadid">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("Kernel32", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern uint GetCurrentThreadId();

	[DllImport("Kernel32", ExactSpelling = true, EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern IntPtr GetModuleHandle(string lpModuleName);

	/// <inheritdoc cref="RegisterRawInputDevices"/>
	public static unsafe bool RegisterRawInputDevices(Span<RAWINPUTDEVICE> pRawInputDevices)
	{
		fixed (RAWINPUTDEVICE* pRawInputDevicesLocal = pRawInputDevices)
		{
			return RegisterRawInputDevices(pRawInputDevicesLocal, (uint)pRawInputDevices.Length, (uint)RAWINPUTDEVICE.SIZE);
		}
	}

	/// <summary>Registers the devices that supply the raw input data.</summary>
	/// <param name="pRawInputDevices">
	/// <para>Type: <b>PCRAWINPUTDEVICE</b> An array of <a href="https://docs.microsoft.com/windows/desktop/api/winuser/ns-winuser-rawinputdevice">RAWINPUTDEVICE</a> structures that represent the devices that supply the raw input.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-registerrawinputdevices#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="uiNumDevices">
	/// <para>Type: <b>UINT</b> The number of <a href="https://docs.microsoft.com/windows/desktop/api/winuser/ns-winuser-rawinputdevice">RAWINPUTDEVICE</a> structures pointed to by <i>pRawInputDevices</i>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-registerrawinputdevices#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="cbSize">
	/// <para>Type: <b>UINT</b> The size, in bytes, of a <see cref="RAWINPUTDEVICE"/> structure.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-registerrawinputdevices#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>BOOL</b> <b>TRUE</b> if the function succeeds; otherwise, <b>FALSE</b>. If the function fails, call <see cref="Marshal.GetLastPInvokeError"/> for more information.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-registerrawinputdevices">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("User32", ExactSpelling = true, SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern unsafe BOOL RegisterRawInputDevices(RAWINPUTDEVICE* pRawInputDevices, uint uiNumDevices, uint cbSize);

	/// <summary>Calls the next handler in a window's subclass chain. The last handler in the subclass chain calls the original window procedure for the window.</summary>
	/// <param name="hWnd">
	/// <para>Type: <b>HWND</b> A handle to the window being subclassed.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-defsubclassproc#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="uMsg">
	/// <para>Type: <b>UINT</b> A value of type unsigned <b>int</b> that specifies a window message.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-defsubclassproc#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="wParam">
	/// <para>Type: <b>WPARAM</b> Specifies additional message information. The contents of this parameter depend on the value of the window message.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-defsubclassproc#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="lParam">
	/// <para>Type: <b>LPARAM</b> Specifies additional message information. The contents of this parameter depend on the value of the window message. Note: On 64-bit versions of Windows LPARAM is a 64-bit value.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-defsubclassproc#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>LRESULT</b> The returned value is specific to the message sent. This value should be ignored.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-defsubclassproc">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("ComCtl32", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern nint DefSubclassProc(IntPtr hWnd, uint uMsg, nuint wParam, nint lParam);

	/// <summary>Installs or updates a window subclass callback.</summary>
	/// <param name="hWnd">
	/// <para>Type: <b>HWND</b> The handle of the window being subclassed.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-setwindowsubclass#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="pfnSubclass">
	/// <para>Type: <b><a href="https://docs.microsoft.com/windows/desktop/api/commctrl/nc-commctrl-subclassproc">SUBCLASSPROC</a></b> A pointer to a window procedure. This pointer and the subclass ID uniquely identify this subclass callback. For the callback function prototype, see <a href="https://docs.microsoft.com/windows/desktop/api/commctrl/nc-commctrl-subclassproc">SUBCLASSPROC</a>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-setwindowsubclass#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="uIdSubclass">
	/// <para>Type: <b>UINT_PTR</b> The subclass ID. This ID together with the subclass procedure uniquely identify a subclass. To remove a subclass, pass the subclass procedure and this value to the <a href="https://docs.microsoft.com/windows/desktop/api/commctrl/nf-commctrl-removewindowsubclass">RemoveWindowSubclass</a> function. This value is passed to the subclass procedure in the uIdSubclass parameter.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-setwindowsubclass#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="dwRefData">
	/// <para>Type: <b>DWORD_PTR</b> <b>DWORD_PTR</b> to reference data. The meaning of this value is determined by the calling application. This value is passed to the subclass procedure in the dwRefData parameter. A different dwRefData is associated with each combination of window handle, subclass procedure and uIdSubclass.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-setwindowsubclass#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>BOOL</b> <b>TRUE</b> if the subclass callback was successfully installed; otherwise, <b>FALSE</b>.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-setwindowsubclass">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("ComCtl32", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern BOOL SetWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, nuint uIdSubclass, nuint dwRefData);

	/// <summary>Removes a subclass callback from a window.</summary>
	/// <param name="hWnd">
	/// <para>Type: <b>HWND</b> The handle of the window being subclassed.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-removewindowsubclass#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="pfnSubclass">
	/// <para>Type: <b><a href="https://docs.microsoft.com/windows/desktop/api/commctrl/nc-commctrl-subclassproc">SUBCLASSPROC</a></b> A pointer to a window procedure. This pointer and the subclass ID uniquely identify this subclass callback. For the callback function prototype, see <a href="https://docs.microsoft.com/windows/desktop/api/commctrl/nc-commctrl-subclassproc">SUBCLASSPROC</a>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-removewindowsubclass#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="uIdSubclass">
	/// <para>Type: <b>UINT_PTR</b> The <b>UINT_PTR</b> subclass ID. This ID and the callback pointer uniquely identify this subclass callback. Note: On 64-bit versions of Windows this is a 64-bit value.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-removewindowsubclass#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>BOOL</b> <b>TRUE</b> if the subclass callback was successfully removed; otherwise, <b>FALSE</b>.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//commctrl/nf-commctrl-removewindowsubclass">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("ComCtl32", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern BOOL RemoveWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, nuint uIdSubclass);

	/// <summary>Calls the default window procedure to provide default processing for any window messages that an application does not process.</summary>
	/// <param name="hWnd">
	/// <para>Type: <b>HWND</b> A handle to the window procedure that received the message.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-defwindowprocw#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="Msg">
	/// <para>Type: <b>UINT</b> The message.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-defwindowprocw#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="wParam">
	/// <para>Type: <b>WPARAM</b> Additional message information. The content of this parameter depends on the value of the <i>Msg</i> parameter.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-defwindowprocw#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="lParam">
	/// <para>Type: <b>LPARAM</b> Additional message information. The content of this parameter depends on the value of the <i>Msg</i> parameter.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-defwindowprocw#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>LRESULT</b> The return value is the result of the message processing and depends on the message.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-defwindowprocw">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("User32", ExactSpelling = true, EntryPoint = "DefWindowProcW")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern nint DefWindowProc(IntPtr hWnd, uint Msg, nuint wParam, nint lParam);

	/// <summary>Retrieves the raw input from the specified device.</summary>
	/// <para>Type: <b>HRAWINPUT</b> A handle to the <a href="https://docs.microsoft.com/windows/desktop/api/winuser/ns-winuser-rawinput">RAWINPUT</a> structure. This comes from the <i>lParam</i> in <see cref="WindowMessage.INPUT"/>.</para>
	/// <returns><typeparamref name="true"/> if successful, otherwise <typeparamref name="false"/>.</returns>
	public static bool GetRawInputData(nint hRawInput, out RAWINPUT pData)
    {
		uint rawInputSize = (uint)Marshal.SizeOf<RAWINPUT>();
		return GetRawInputData(hRawInput, 0x10000003u, out pData, ref rawInputSize, (uint)RAWINPUTHEADER.SIZE) != unchecked((uint)-1);
    }

	/// <summary>Retrieves the raw input from the specified device.</summary>
	/// <param name="hRawInput">
	/// <para>Type: <b>HRAWINPUT</b> A handle to the <a href="https://docs.microsoft.com/windows/desktop/api/winuser/ns-winuser-rawinput">RAWINPUT</a> structure. This comes from the <i>lParam</i> in <a href="https://docs.microsoft.com/windows/desktop/inputdev/wm-input">WM_INPUT</a>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getrawinputdata#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="uiCommand">
	/// Type: <b>UINT</b> The command flag. This parameter can be one of the following values:
	/// <code>
	/// +-------------------------+---------------------------------------------------------+
	/// | RID_HEADER = 0x10000005u | Get the header information from the <see cref="RAWINPUT"/> structure. |
	/// +-------------------------+---------------------------------------------------------+
	/// | RID_INPUT = 0x10000003u  | Get the raw data from the <see cref="RAWINPUT"/> structure.           |
	/// +-------------------------+---------------------------------------------------------+
	/// </code>
	/// </param>
	/// <param name="pData">
	/// <para>Type: <b>LPVOID</b> A pointer to the data that comes from the <a href="https://docs.microsoft.com/windows/desktop/api/winuser/ns-winuser-rawinput">RAWINPUT</a> structure. This depends on the value of <i>uiCommand</i>. If <i>pData</i> is <b>NULL</b>, the required size of the buffer is returned in *<i>pcbSize</i>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getrawinputdata#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="pcbSize">
	/// <para>Type: <b>PUINT</b> The size, in bytes, of the data in <i>pData</i>.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getrawinputdata#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <param name="cbSizeHeader">
	/// <para>Type: <b>UINT</b> The size, in bytes, of the <a href="https://docs.microsoft.com/windows/desktop/api/winuser/ns-winuser-rawinputheader">RAWINPUTHEADER</a> structure.</para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getrawinputdata#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns>
	/// <para>Type: <b>UINT</b> If <i>pData</i> is <b>NULL</b> and the function is successful, the return value is 0. If <i>pData</i> is not <b>NULL</b> and the function is successful, the return value is the number of bytes copied into pData. If there is an error, the return value is (<b>UINT</b>)-1.</para>
	/// </returns>
	/// <remarks>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getrawinputdata">Learn more about this API from docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("User32", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern uint GetRawInputData(nint hRawInput, uint uiCommand, out RAWINPUT pData, ref uint pcbSize, uint cbSizeHeader);
}
