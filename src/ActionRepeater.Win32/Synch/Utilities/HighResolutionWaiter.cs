using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static ActionRepeater.Win32.PInvoke;

namespace ActionRepeater.Win32.Synch.Utilities;

public sealed class HighResolutionWaiter : IDisposable
{
    private IntPtr _timerHandle;
    private bool _isWaiting;

    private bool _disposed;

    public HighResolutionWaiter()
    {
        _timerHandle = CreateWaitableTimerEx(IntPtr.Zero, null, WaitableTimerFlags.HIGH_RESOLUTION, AccessRights.DELETE | AccessRights.SYNCHRONIZE | AccessRights.TIMER_MODIFY_STATE);
        if (_timerHandle == IntPtr.Zero)
        {
            throw new Win32Exception();
        }
    }

    public unsafe void Wait(long milliseconds)
    {
        if (milliseconds <= 0) return;

        WaitCore(GetRelativeTimeMS(milliseconds));
    }

    public unsafe void WaitNS(long nanoseconds)
    {
        if (nanoseconds <= 0) return;

        WaitCore(GetRelativeTimeNS(nanoseconds));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void WaitCore(long time)
    {
        Debug.Assert(!_isWaiting);

        _isWaiting = true;

        if (!SetWaitableTimer(_timerHandle, &time, 0, null, null, false))
        {
            throw new Win32Exception();
        }

        if (WaitForSingleObject(_timerHandle, MACROS.INFINITE) != WaitResult.OBJECT_0)
        {
            throw new Win32Exception();
        }

        _isWaiting = false;
    }

    /// <remarks>Calling <see cref="Cancel"/> when not waiting is a no-op.</remarks>
    public unsafe void Cancel()
    {
        if (!_isWaiting) return;

        long time = -1;
        if (!SetWaitableTimer(_timerHandle, &time, 0, null, null, false))
        {
            throw new Win32Exception();
        }

        _isWaiting = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetRelativeTimeMS(long milliseconds)
    {
        // negative == relative time (positive == absolute)
        // we want 100 nanosecond intervals, so multiply by 1,000,000 to get nanoseconds
        // and divide by 100; == multiplying by 10,000
        return -(milliseconds * 10_000L);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetRelativeTimeNS(long nanoseconds) => unchecked(-(nanoseconds / 100L));

    private void DisposeCore()
    {
        if (_disposed) return;

        CloseHandle(_timerHandle);
        _timerHandle = IntPtr.Zero;

        _disposed = true;
    }

    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    ~HighResolutionWaiter()
    {
        DisposeCore();
    }
}
