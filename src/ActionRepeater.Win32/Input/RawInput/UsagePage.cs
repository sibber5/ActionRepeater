namespace ActionRepeater.Win32.Input;

/// <summary>
/// HID usages are organized into usage pages of related controls. A specific control usage is defined by its usage page, a usage ID, a name, and a description.<br/>
/// A usage page value is a 16-bit unsigned value (<see cref="ushort"/>).
/// </summary>
public enum UsagePage : ushort
{
    /// <summary>
    /// Generic Desktop Controls
    /// </summary>
    GenericDesktopControl = 0x01,

    /// <summary>
    /// Game Controls
    /// </summary>
    GameControl = 0x05,

    /// <summary>
    /// LEDs
    /// </summary>
    LED = 0x08,

    /// <summary>
    /// Button
    /// </summary>
    Button = 0x09,
}
