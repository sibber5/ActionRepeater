using System.Runtime.InteropServices;
using Debug = System.Diagnostics.Debug;

namespace ActionRepeater.Win32.WindowsAndMessages.HookHelpers;

[System.Runtime.Versioning.SupportedOSPlatform("windows5.1.2600")]
public static class JournalRecordHook
{
    public static event EventHandler<EVENTMSG>? MessageRecieved;

    private static readonly HOOKPROC _hookProc = new(JournalRecordProc);
    private static SafeHookHandle? _hookProcHandle = null;

    /// <summary>
    /// <typeparamref name="true"/> if a system-modal dialog box (system dialog box that blocks the whole UI until closed e.g. UAC dialog) is open.
    /// </summary>
    private static bool _isSystemModal = false;

    public static void Hook()
    {
        if (_hookProcHandle is not null)
        {
            return;
        }

        //_hookProcHandle = NativeMethods.SetWindowsHookEx(WindowsHookType.JOURNALRECORD, _hookProc, null, NativeMethods.GetCurrentThreadId());
        IntPtr dllHandle = PInvoke.GetModuleHandle(System.Reflection.Assembly.GetCallingAssembly().Location);
        if (dllHandle == IntPtr.Zero)
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError());
        }
        _hookProcHandle = new SafeHookHandle(PInvoke.SetWindowsHookEx(WindowsHookType.JOURNALRECORD, _hookProc, dllHandle, 0), true);
        if (_hookProcHandle.IsInvalid)
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError());
        }
        Debug.WriteLine($"Installed {nameof(JournalRecordHook)}");
    }

    public static void Unhook()
    {
        if (_hookProcHandle is null)
        {
            return;
        }

        _hookProcHandle.Close();
        _hookProcHandle = null;
        Debug.WriteLine($"Uninstalled {nameof(JournalRecordHook)}");
    }

    /// <summary>
    /// An application-defined or library-defined callback function used with the <see cref="PInvoke.SetWindowsHookEx(WindowsHookType, HOOKPROC, SafeHandle, uint)"/> function. The function records messages the system removes from the system message queue. Later, an application can use a <see cref="JournalPlaybackProc"/> hook procedure to play back the messages.
    /// </summary>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/winmsg/journalrecordproc">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    /// <param name="code">
    /// Specifies how to process the message. If <paramref name="code"/> is less than zero, the hook procedure must pass the message to the <see cref="PInvoke.CallNextHookEx(int, nuint, nint)"/> function without further processing and should return the value returned by <see cref="PInvoke.CallNextHookEx(int, nuint, nint)"/>. This parameter can be one of the following values:
    /// <code>
    /// ╔══════════════════════╦════════════════════════════════════════════════════╗
    /// ║                      ║ The <paramref name="lParam"/> parameter is a pointer to an <b>EVENTMSG</b>   ║
    /// ║                      ║ structure containing information about a message   ║
    /// ║ <see cref="HookCode.ACTION"/>      ║ removed from the system queue. The hook procedure  ║
    /// ║                      ║ must record the contents of the structure by       ║
    /// ║                      ║ copying them to a buffer or file.                  ║
    /// ╠══════════════════════╬════════════════════════════════════════════════════╣
    /// ║ <see cref="HookCode.SYSMODALOFF"/> ║ A system-modal dialog box has been destroyed.      ║
    /// ║                      ║ The hook procedure must resume recording.          ║
    /// ╠══════════════════════╬════════════════════════════════════════════════════╣
    /// ║                      ║ A system-modal dialog box is being displayed.      ║
    /// ║ <see cref="HookCode.SYSMODALON"/>  ║ Until the dialog box is destroyed, the hook        ║
    /// ║                      ║ procedure must stop recording.                     ║
    /// ╚══════════════════════╩════════════════════════════════════════════════════╝
    /// </code>
    /// </param>
    /// <param name="wParam">This parameter is not used.</param>
    /// <param name="lParam">A pointer to an <b>EVENTMSG</b> structure that contains the message to be recorded.</param>
    /// <returns>The return value is ignored.</returns>
    private static nint JournalRecordProc(int code, nuint wParam, nint lParam)
    {
        if (code >= 0)
        {
            Debug.WriteLine("poc recieved message.");
            switch ((HookCode)code)
            {
                case HookCode.ACTION:
                    if (_isSystemModal)
                    {
                        break;
                    }

                    MessageRecieved?.Invoke(typeof(JournalRecordHook), Marshal.PtrToStructure<EVENTMSG>(lParam));
                    break;

                case HookCode.SYSMODALOFF:
                    _isSystemModal = false;
                    break;

                case HookCode.SYSMODALON:
                    _isSystemModal = true;
                    break;
            }
        }

        return PInvoke.CallNextHookEx(code, wParam, lParam);
    }
}
