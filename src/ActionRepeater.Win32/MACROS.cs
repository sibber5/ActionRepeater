namespace ActionRepeater.Win32;

public static class MACROS
{
    public const uint INFINITE = unchecked((uint)-1);

    // The following are defined in WinUser.h

    public const uint KF_EXTENDED = 0x0100u;
    public const uint KF_DLGMODE = 0x0800u;
    public const uint KF_MENUMODE = 0x1000u;
    public const uint KF_ALTDOWN = 0x2000u;
    public const uint KF_REPEAT = 0x4000u;
    public const uint KF_UP = 0x8000u;

    // The following are defined in minwindef.h

    public static ushort LOWORD(nuint l) => unchecked((ushort)(((nint)l) & 0xffff));
    public static ushort LOWORD(nint l) => unchecked((ushort)(l & 0xffff));

    public static ushort HIWORD(nuint l) => unchecked((ushort)((((nint)l) >> 16) & 0xffff));
    public static ushort HIWORD(nint l) => unchecked((ushort)((l >> 16) & 0xffff));

    public static byte LOBYTE(nuint l) => unchecked((byte)(((nint)l) & 0xff));
    public static byte LOBYTE(nint l) => unchecked((byte)(l & 0xff));

    public static byte HIBYTE(nuint l) => unchecked((byte)((((nint)l) >> 8) & 0xff));
    public static byte HIBYTE(nint l) => unchecked((byte)((l >> 8) & 0xff));
}
