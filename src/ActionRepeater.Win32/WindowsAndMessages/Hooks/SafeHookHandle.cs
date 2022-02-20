using System.Runtime.InteropServices;

namespace ActionRepeater.Win32.WindowsAndMessages;

/// <summary>
/// Represents a Win32 handle that can be closed with <see cref="PInvoke.UnhookWindowsHookEx"/> (by calling <see cref="ReleaseHandle"/>).
/// </summary>
public class SafeHookHandle : SafeHandle
{
	public SafeHookHandle()
		: base(IntPtr.Zero, true)
	{
	}

	public SafeHookHandle(IntPtr preexistingHandle, bool ownsHandle = true)
		: base(IntPtr.Zero, ownsHandle)
	{
		this.SetHandle(preexistingHandle);
	}

	public override bool IsInvalid => this.handle == default || this.handle == IntPtr.Zero;

	protected override bool ReleaseHandle() => PInvoke.UnhookWindowsHookEx(this.handle);
}
