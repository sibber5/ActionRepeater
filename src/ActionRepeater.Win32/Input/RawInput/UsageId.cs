namespace ActionRepeater.Win32.Input;

/// <summary>
/// In the context of a usage page, a valid usage identifier, or usage ID, indicates a usage in a usage page.<br/>
/// A usage ID of zero is reserved.<br/>
/// A usage ID value is an unsigned 16-bit value (<see cref="ushort"/>).
/// </summary>
public enum UsageId : ushort
{
    Pointer = 0x01,
    Mouse = 0x02,
    Joystick = 0x04,
    GamePad = 0x05,
    Keyboard = 0x06,
    Keypad = 0x07,
    MultiAxisController = 0x08,
}
