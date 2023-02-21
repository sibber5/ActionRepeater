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
/// Event arguments for the <see cref="WindowMessageMonitor.WindowMessageReceived"/> event.
/// </summary>
public sealed class WindowMessageEventArgs : EventArgs
{
    internal WindowMessageEventArgs(nint hwnd, uint messageId, nuint wParam, nint lParam)
    {
        Message = new MSG()
        {
            hwnd = hwnd,
            message = messageId,
            wParam = wParam,
            lParam = lParam
        };
    }

    /// <summary>
    /// The result after processing the message. Use this to set the return result, after also setting <see cref="Handled"/> to <c>true</c>.
    /// </summary>
    /// <seealso cref="Handled"/>
    public nint Result { get; set; }

    /// <summary>
    /// Indicates whether this message was handled and the <see cref="Result"/> value should be returned.
    /// </summary>
    /// <remarks><c>True</c> is the message was handled and the <see cref="Result"/> should be returned, otherwise <c>false</c> and continue processing this message by other subsclasses.</remarks>
    /// <seealso cref="Result"/>
    public bool Handled { get; set; }

    /// <summary>
    /// The Windows WM Message
    /// </summary>
    public MSG Message { get; }

    public WindowMessage MessageType => (WindowMessage)Message.message;
}
