using System;
using ActionRepeater.Win32;
using ActionRepeater.Win32.WindowsAndMessages;

namespace ActionRepeater.Messaging;

// From https://github.com/dotMorten/WinUIEx by dotMorten (MIT Licence)

/// <summary>
/// The message monitor allows you to monitor all WM_MESSAGE events for a given window.
/// </summary>
public sealed class WindowMessageMonitor : IDisposable
{
    private readonly IntPtr _hwnd = IntPtr.Zero;
    private SUBCLASSPROC? callback;
    private readonly object _lockObject = new();

    /// <summary>
    /// Initialize a new instance of the <see cref="WindowMessageMonitor"/> class.
    /// </summary>
    /// <param name="window">The window to listen to messages for</param>
    public WindowMessageMonitor(Microsoft.UI.Xaml.Window window) : this(WinRT.Interop.WindowNative.GetWindowHandle(window)) { }

    /// <summary>
    /// Initialize a new instance of the <see cref="WindowMessageMonitor"/> class.
    /// </summary>
    /// <param name="hwnd">The window handle to listen to messages for</param>
    public WindowMessageMonitor(IntPtr hwnd)
    {
        _hwnd = hwnd;
    }

    private event EventHandler<WindowMessageEventArgs>? _windowMessageReceived;
    /// <summary>
    /// Event raised when a windows message is received.
    /// </summary>
    public event EventHandler<WindowMessageEventArgs>? WindowMessageReceived
    {
        add
        {
            if (_windowMessageReceived is null)
            {
                Subscribe();
            }
            _windowMessageReceived += value;
        }
        remove
        {
            _windowMessageReceived -= value;
            if (_windowMessageReceived is null)
            {
                Unsubscribe();
            }
        }
    }

    private nint NewWindowProc(IntPtr hWnd, uint uMsg, nuint wParam, nint lParam, nuint uIdSubclass, nuint dwRefData)
    {
        if (_windowMessageReceived is not null)
        {
            var args = new WindowMessageEventArgs(hWnd, uMsg, wParam, lParam);
            _windowMessageReceived.Invoke(this, args);
            if (args.Handled) return (int)args.Result;
        }
        return PInvoke.DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    private void Subscribe()
    {
        lock (_lockObject)
        {
            if (callback == null)
            {
                callback = new SUBCLASSPROC(NewWindowProc);
                PInvoke.SetWindowSubclass(_hwnd, callback, 101, 0);
            }
        }
    }

    private void Unsubscribe()
    {
        lock (_lockObject)
        {
            if (callback != null)
            {
                PInvoke.RemoveWindowSubclass(_hwnd, callback, 101);
                callback = null;
            }
        }
    }

    public void Dispose()
    {
        if (_windowMessageReceived is not null) Unsubscribe();
    }
}
