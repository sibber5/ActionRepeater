namespace ActionRepeater.Win32.Input;

public enum RawInputType : uint
{
    /// <summary>
    /// Raw input comes from the mouse.
    /// </summary>
    Mouse = 0,
    /// <summary>
    /// Raw input comes from the keyboard.
    /// </summary>
    Keyboard = 1,
    /// <summary>
    /// (Human Interface Device) Raw input comes from some device that is not a keyboard or a mouse.
    /// </summary>
    HID = 2,
}
