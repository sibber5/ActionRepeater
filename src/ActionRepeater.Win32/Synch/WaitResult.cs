namespace ActionRepeater.Win32.Synch;

public enum WaitResult : uint
{
    /// <summary>
    /// <para>The specified object is a mutex object that was not released by the thread that owned the mutex object before the owning thread terminated. Ownership of the mutex object is granted to the calling thread and the mutex state is set to nonsignaled.</para>
    /// <para>If the mutex was protecting persistent state information, you should check it for consistency.</para>
    /// </summary>
    ABANDONED = 0x00000080u,
    /// <summary>
    /// The state of the specified object is signaled.
    /// </summary>
    OBJECT_0 = 0x00000000u,
    /// <summary>
    /// The time-out interval elapsed, and the object's state is nonsignaled.
    /// </summary>
    TIMEOUT = 0x00000102u,
    /// <summary>
    /// The function has failed. To get extended error information, call <see cref="System.Runtime.InteropServices.Marshal.GetLastPInvokeError"/>.
    /// </summary>
    FAILED = unchecked((uint)0xFFFFFFFF),
}
