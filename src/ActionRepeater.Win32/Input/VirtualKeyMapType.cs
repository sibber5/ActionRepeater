namespace ActionRepeater.Win32.Input;

/// <summary>
/// <para>Used by <see cref="PInvoke.MapVirtualKey"/> to tell which translation to perform.</para>
/// <para><see href="https://docs.microsoft.com/windows/win32/api/winuser/nf-winuser-mapvirtualkeyw#parameters">Read more on docs.microsoft.com</see></para>
/// </summary>
public enum VirtualKeyMapType : uint
{
    /// <summary>
    /// The <paramref name="uCode"/> parameter is a virtual-key code and is translated into a scan code.<br/>
    /// If it is a virtual-key code that does not distinguish between left- and right-hand keys, the left-hand scan code is returned.
    /// </summary>
    VK_TO_VSC = 0u,

    /// <summary>
    /// The <paramref name="uCode"/> parameter is a scan code and is translated into a virtual-key code that does not distinguish between left- and right-hand keys.
    /// </summary>
    VSC_TO_VK = 1u,

    /// <summary>
    /// The <paramref name="uCode"/> parameter is a virtual-key code and is translated into an unshifted character value in the low order word of the return value.<br/>
    /// Dead keys (diacritics) are indicated by setting the top bit of the return value.
    /// </summary>
    VK_TO_CHAR = 2u,

    /// <summary>
    /// The <paramref name="uCode"/> parameter is a scan code and is translated into a virtual-key code that distinguishes between left- and right-hand keys.
    /// </summary>
    VSC_TO_VK_EX = 3u,

    /// <summary>
    /// <b>Windows Vista and later</b>: The <paramref name="uCode"/> parameter is a virtual-key code and is translated into a scan code.<br/>
    /// If it is a virtual-key code that does not distinguish between left- and right-hand keys, the left-hand scan code is returned. If the scan code is an extended scan code, the high byte of the uCode value can contain either 0xe0 or 0xe1 to specify the extended scan code.
    /// </summary>
    VK_TO_VSC_EX = 4u,
}
