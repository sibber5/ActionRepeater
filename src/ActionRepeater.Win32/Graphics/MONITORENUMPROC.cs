using System;
using System.Runtime.InteropServices;

namespace ActionRepeater.Win32.Graphics;

[UnmanagedFunctionPointer(CallingConvention.Winapi)]
public delegate bool MONITORENUMPROC(IntPtr hMonitor, IntPtr hdc, IntPtr rect, nint lParam);
