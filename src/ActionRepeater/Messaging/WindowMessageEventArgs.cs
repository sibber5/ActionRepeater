using System;
using ActionRepeater.Win32.WindowsAndMessages;

namespace ActionRepeater.Messaging;

// From https://github.com/dotMorten/WinUIEx by dotMorten (MIT Licence)

/// <summary>
/// Event arguments for the <see cref="WindowMessageMonitor.WindowMessageReceived"/> event.
/// </summary>
public sealed class WindowMessageEventArgs : EventArgs
{
    internal WindowMessageEventArgs(IntPtr hwnd, uint messageId, nuint wParam, nint lParam)
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

    internal WindowMessage MessageType => (WindowMessage)Message.message;
}
