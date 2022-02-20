using System.Runtime.InteropServices;
using Debug = System.Diagnostics.Debug;

namespace ActionRepeater.Win32.WindowsAndMessages.HookHelpers;

[System.Runtime.Versioning.SupportedOSPlatform("windows5.1.2600")]
public static class LowLevelKeyboardHook
{
    public static event EventHandler<(WindowMessage windowMessage, KBDLLHOOKSTRUCT eventInfo)>? MessageRecieved;
    public static event EventHandler<(nuint wParam, nint lParam)>? MessageRecievedRaw;

    private static readonly HOOKPROC _hookProc = new(LowLevelKeyboardProc);
    private static SafeHookHandle? _hookProcHandle = null;

    public static void Hook()
    {
        if (_hookProcHandle is not null)
        {
            return;
        }

        IntPtr dllHandle = PInvoke.GetModuleHandle(System.Reflection.Assembly.GetCallingAssembly().Location);
        if (dllHandle == IntPtr.Zero)
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError());
        }
        _hookProcHandle = new SafeHookHandle(PInvoke.SetWindowsHookEx(WindowsHookType.KEYBOARD_LL, _hookProc, dllHandle, 0), true);
        if (_hookProcHandle.IsInvalid)
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError());
        }
        //Debug.WriteLine($"Installed {nameof(LowLevelKeyboardHook)}");
    }

    public static void Unhook()
    {
        if (_hookProcHandle is null)
        {
            return;
        }

        _hookProcHandle.Close();
        _hookProcHandle = null;
        //Debug.WriteLine($"Uninstalled {nameof(LowLevelKeyboardHook)}");
    }

    private static nint LowLevelKeyboardProc(int code, nuint wParam, nint lParam)
    {
        if (code == 0)
        {
            MessageRecievedRaw?.Invoke(typeof(LowLevelKeyboardHook), (wParam, lParam));
            MessageRecieved?.Invoke(typeof(LowLevelKeyboardHook), ((WindowMessage)(uint)wParam, Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam)));
        }

        return PInvoke.CallNextHookEx(code, wParam, lParam);
    }
}
