using System;

namespace ActionRepeater.Win32.WindowsAndMessages.Utilities;

// From https://github.com/dotMorten/WinUIEx by dotMorten
// MIT License
// 
// Copyright(c) 2021 Morten Nielsen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

/// <summary>
/// The message monitor allows you to monitor all WM_MESSAGE events for a given window.
/// </summary>
public sealed class WindowMessageMonitor : IDisposable
{
    private readonly nint _hwnd = nint.Zero;
    private SUBCLASSPROC? callback;
    private readonly object _lockObject = new();

    /// <summary>
    /// Initialize a new instance of the <see cref="WindowMessageMonitor"/> class.
    /// </summary>
    /// <param name="hwnd">The window handle to listen to messages for</param>
    public WindowMessageMonitor(nint hwnd)
    {
        _hwnd = hwnd;
    }

    public void Dispose()
    {
        if (_windowMessageReceived is not null) Unsubscribe();
    }

    private event EventHandler<WindowMessageEventArgs>? _windowMessageReceived;

    /// <summary>
    /// Event raised when a windows message is received.
    /// </summary>
    public event EventHandler<WindowMessageEventArgs> WindowMessageReceived
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

    private nint NewWindowProc(nint hWnd, uint uMsg, nuint wParam, nint lParam, nuint uIdSubclass, nuint dwRefData)
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
}
