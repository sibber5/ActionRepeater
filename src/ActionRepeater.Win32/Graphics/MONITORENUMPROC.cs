using System.Runtime.InteropServices;

namespace ActionRepeater.Win32.Graphics;

[UnmanagedFunctionPointer(CallingConvention.Winapi)]
public delegate bool MONITORENUMPROC(nint hMonitor, nint hdc, nint rect, nint lParam);
