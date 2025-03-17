using System.Runtime.InteropServices;

namespace ActionRepeater.Win32;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct DWM_BLURBEHIND
{
    /// <summary>A bitwise combination of <a href="https://docs.microsoft.com/windows/desktop/dwm/dwm-bb-constants">DWM Blur Behind</a> constant values that indicates which of the members of this structure have been set.</summary>
    public uint dwFlags;

    /// <summary><b>TRUE</b> to register the window handle to DWM blur behind; <b>FALSE</b> to unregister the window handle from DWM blur behind.</summary>
    public int fEnable;

    /// <summary>The region within the client area where the blur behind will be applied. A <b>NULL</b> value will apply the blur behind the entire client area.</summary>
    public nint hRgnBlur;

    /// <summary><b>TRUE</b> if the window's colorization should transition to match the maximized windows; otherwise, <b>FALSE</b>.</summary>
    public int fTransitionOnMaximized;

    public DWM_BLURBEHIND(uint dwFlags = default, bool fEnable = default, nint hRgnBlur = default, bool fTransitionOnMaximized = default)
    {
        this.dwFlags = dwFlags;
        this.fEnable = fEnable ? 1 : 0;
        this.hRgnBlur = hRgnBlur;
        this.fTransitionOnMaximized = fTransitionOnMaximized ? 1 : 0;
    }
}
