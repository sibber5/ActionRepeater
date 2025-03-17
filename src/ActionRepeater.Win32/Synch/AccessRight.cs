using System;

namespace ActionRepeater.Win32.Synch;

[Flags]
public enum AccessRights : uint
{
    // standard access rights:

    /// <summary>
    /// Required to delete the object.
    /// </summary>
    DELETE = 0x00010000,
    /// <summary>
    /// <para>Required to read information in the security descriptor for the object, not including the information in the SACL. To read or write the SACL, you must request the ACCESS_SYSTEM_SECURITY access right.</para>
    /// <para>For more information, see <see href="https://learn.microsoft.com/en-us/windows/win32/secauthz/sacl-access-right">SACL Access Right</see>.</para>
    /// </summary>
    READ_CONTROL = 0x00020000,
    /// <summary>
    /// The right to use the object for synchronization. This enables a thread to wait until the object is in the signaled state.
    /// </summary>
    SYNCHRONIZE = 0x00100000,
    /// <summary>
    /// Required to modify the DACL in the security descriptor for the object.
    /// </summary>
    WRITE_DAC = 0x00040000,
    /// <summary>
    /// Required to change the owner in the security descriptor for the object.
    /// </summary>
    WRITE_OWNER = 0x00080000,

    // waitable timer rights: (these rights are supported in addition to the standard access rights)

    /// <summary>
    /// All possible access rights for a waitable timer object. Use this right only if your application requires access beyond that granted by the standard access rights and <see cref="TIMER_MODIFY_STATE"/>. Using this access right increases the possibility that your application must be run by an Administrator.
    /// </summary>
    TIMER_ALL_ACCESS = 0x1F0003,
    /// <summary>
    /// Modify state access, which is required for the <b>SetWaitableTimer</b> and <b>CancelWaitableTimer</b> functions.
    /// </summary>
    TIMER_MODIFY_STATE = 0x0002,
    /// <summary>
    /// Reserved for future use.
    /// </summary>
    TIMER_QUERY_STATE = 0x0001,
}
