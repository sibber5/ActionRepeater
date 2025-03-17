using System.Runtime.InteropServices;

namespace ActionRepeater.Win32;

/// <summary>
/// Represents a Win32 handle that can be closed with <see cref="PInvoke.DeleteObject(winmdroot.Graphics.Gdi.HGDIOBJ)"/>.
/// </summary>
public class DeleteObjectSafeHandle
    : SafeHandle
{
    private static readonly nint INVALID_HANDLE_VALUE = new(-1L);

    public DeleteObjectSafeHandle() : base(INVALID_HANDLE_VALUE, true)
    {
    }

    public DeleteObjectSafeHandle(nint preexistingHandle, bool ownsHandle = true) : base(INVALID_HANDLE_VALUE, ownsHandle)
    {
        SetHandle(preexistingHandle);
    }

    public override bool IsInvalid => handle.ToInt64() == -1L || handle.ToInt64() == 0L;

    protected override bool ReleaseHandle() => PInvoke.DeleteObject(handle);
}
