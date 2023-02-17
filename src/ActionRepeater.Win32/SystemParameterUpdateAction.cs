namespace ActionRepeater.Win32;

public enum SystemParameterUpdateAction : uint
{
    None = 0,
    /// <summary>
    /// Writes the new system-wide parameter setting to the user profile. 
    /// </summary>
    UPDATEINIFILE = 0x00000001U,
    /// <summary>
    /// Broadcasts the <i>WM_SETTINGCHANGE</i> message after updating the user profile.
    /// </summary>
    SENDCHANGE = 0x00000002U,
    /// <summary>
    /// Same as <see cref="SENDCHANGE"/>.
    /// </summary>
    SENDWININICHANGE = SENDCHANGE,
}
