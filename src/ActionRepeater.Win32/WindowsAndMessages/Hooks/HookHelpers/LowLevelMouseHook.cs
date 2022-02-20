using System.Runtime.InteropServices;
using Debug = System.Diagnostics.Debug;

namespace ActionRepeater.Win32.WindowsAndMessages.HookHelpers;

[System.Runtime.Versioning.SupportedOSPlatform("windows5.1.2600")]
public static class LowLevelMouseHook
{
    public static event EventHandler<(WindowMessage windowMessage, MSLLHOOKSTRUCT eventInfo)>? MessageRecieved;

    private static readonly HOOKPROC _hookProc = new(LowLevelMouseProc);
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
        _hookProcHandle = new SafeHookHandle(PInvoke.SetWindowsHookEx(WindowsHookType.MOUSE_LL, _hookProc, dllHandle, 0), true);
        if (_hookProcHandle.IsInvalid)
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError());
        }
        //Debug.WriteLine($"Installed {nameof(LowLevelMouseHook)}");
    }

    public static void Unhook()
    {
        if (_hookProcHandle is null)
        {
            return;
        }

        _hookProcHandle.Close();
        _hookProcHandle = null;
        //Debug.WriteLine($"Uninstalled {nameof(LowLevelMouseHook)}");
    }

    private static nint LowLevelMouseProc(int code, nuint wParam, nint lParam)
    {
        if (code == 0)
        {
            MessageRecieved?.Invoke(typeof(LowLevelMouseHook), ((WindowMessage)(uint)wParam, Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam)));
        }

        return PInvoke.CallNextHookEx(code, wParam, lParam);
    }
}
