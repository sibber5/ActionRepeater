using System.Runtime.InteropServices;

namespace ActionRepeater.Win32.WindowsAndMessages;

[UnmanagedFunctionPointer(CallingConvention.Winapi)]
public unsafe delegate nint HOOKPROC(int code, nuint wParam, nint lParam);

/// <summary>
/// An application-defined or library-defined callback function used with the <see cref="PInvoke.SetWindowsHookEx(WindowsHookType, HOOKPROC, SafeHandle, uint)"/> function. Typically, an application uses this function to play back a series of mouse and keyboard messages recorded previously by the JournalRecordProc hook procedure.<br/>
/// As long as a <see cref="JournalRecordProc"/> hook procedure is installed, regular mouse and keyboard input is disabled.
/// </summary>
/// <remarks>
/// <para><see href="https://docs.microsoft.com/windows/win32/winmsg/journalplaybackproc">Learn more about this API from docs.microsoft.com</see>.</para>
/// </remarks>
/// <param name="code">
/// Specifies how to process the message. If <paramref name="code"/> is less than zero, the hook procedure must pass the message to the <see cref="PInvoke.CallNextHookEx(int, nuint, nint)"/>
/// function without further processing and should return the value returned by <see cref="PInvoke.CallNextHookEx(int, nuint, nint)"/>. This parameter can be one of the following values:
/// <code>
/// ╔══════════════════════╦══════════════════════════════════════════════════╗
/// ║                      ║ The hook procedure must copy the current mouse   ║
/// ║ <see cref="HookCode.GETNEXT"/>     ║ or keyboard message to the <b>EVENTMSG</b> structure    ║
/// ║                      ║ pointed to by the <see cref="lParam"/> parameter.              ║
/// ╠══════════════════════╬══════════════════════════════════════════════════╣
/// ║                      ║ An application has called the PeekMessage        ║
/// ║ <see cref="HookCode.NOREMOVE"/>    ║ function with wRemoveMsg set to PM_NOREMOVE,     ║
/// ║                      ║ indicating that the message is not removed from  ║
/// ║                      ║ the message queue after PeekMessage processing.  ║
/// ╠══════════════════════╬══════════════════════════════════════════════════╣
/// ║                      ║ The hook procedure must prepare to copy the      ║
/// ║                      ║ next mouse or keyboard message to the <b>EVENTMSG</b>   ║
/// ║ <see cref="HookCode.SKIP"/>        ║ structure pointed to by <see cref="lParam"/>. Upon receiving   ║
/// ║                      ║ the HC_GETNEXT code, the hook procedure must     ║
/// ║                      ║ copy the message to the structure.               ║
/// ╠══════════════════════╬══════════════════════════════════════════════════╣
/// ║                      ║ A system-modal dialog box has been destroyed.    ║
/// ║ <see cref="HookCode.SYSMODALOFF"/> ║ The hook procedure must resume playing back      ║
/// ║                      ║ the messages.                                    ║
/// ╠══════════════════════╬══════════════════════════════════════════════════╣
/// ║                      ║ A system-modal dialog box is being displayed.    ║
/// ║ <see cref="HookCode.SYSMODALON"/>  ║ Until the dialog box is destroyed, the hook      ║
/// ║                      ║ procedure must stop playing back messages.       ║
/// ╚══════════════════════╩══════════════════════════════════════════════════╝
/// </code>
/// </param>
/// <param name="wParam">This parameter is not used.</param>
/// <param name="lParam">A pointer to an <b>EVENTMSG</b> structure that represents a message being processed by the hook procedure.<br/>
/// This parameter is valid only when the code parameter is <see cref="HookCode.GETNEXT"/>.</param>
/// <returns>
/// <para>To have the system wait before processing the message, the return value must be the amount of time, in clock ticks, that the system should wait.</para>
/// <para>(This value can be computed by calculating the difference between the time members in the current and previous input messages.)</para>
/// <para>To process the message immediately, the return value should be zero. The return value is used only if the hook code is <see cref="HookCode.GETNEXT"/>; otherwise, it is ignored.</para>
/// </returns>
[UnmanagedFunctionPointer(CallingConvention.Winapi)]
public unsafe delegate nint JournalPlaybackProc(int code, nuint wParam, nint lParam);
