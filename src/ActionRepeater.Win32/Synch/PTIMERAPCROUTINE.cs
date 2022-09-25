using System.Runtime.InteropServices;

namespace ActionRepeater.Win32.Synch;

[UnmanagedFunctionPointer(CallingConvention.Winapi)]
public unsafe delegate void PTIMERAPCROUTINE([Optional] void* lpArgToCompletionRoutine, uint dwTimerLowValue, uint dwTimerHighValue);
