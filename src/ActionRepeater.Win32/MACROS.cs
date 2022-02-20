namespace ActionRepeater.Win32;

public static class MACROS
{
    // The following are defined in WinUser.h

    public const uint KF_EXTENDED = 0x0100;
    public const uint KF_DLGMODE = 0x0800;
    public const uint KF_MENUMODE = 0x1000;
    public const uint KF_ALTDOWN = 0x2000;
    public const uint KF_REPEAT = 0x4000;
    public const uint KF_UP = 0x8000;

    // The following are defined in minwindef.h

    public unsafe static ushort LOWORD(nuint l) => unchecked((ushort)(((nint)l) & 0xffff));
    public unsafe static ushort LOWORD(nint l) => unchecked((ushort)(l & 0xffff));

    public unsafe static ushort HIWORD(nuint l) => unchecked((ushort)((((nint)l) >> 16) & 0xffff));
    public unsafe static ushort HIWORD(nint l) => unchecked((ushort)((l >> 16) & 0xffff));

    public unsafe static byte LOBYTE(nuint l) => unchecked((byte)(((nint)l) & 0xff));
    public unsafe static byte LOBYTE(nint l) => unchecked((byte)(l & 0xff));

    public unsafe static byte HIBYTE(nuint l) => unchecked((byte)((((nint)l) >> 8) & 0xff));
    public unsafe static byte HIBYTE(nint l) => unchecked((byte)((l >> 8) & 0xff));
}
