using System;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.InteropServices;
using ActionRepeater.Win32.Input;

namespace ActionRepeater.Win32;

public static partial class PInvoke
{
    public static class Helpers
    {
        /// <summary>
        /// Synthesize a mouse event.
        /// </summary>
        /// <param name="eventFlags">The mouse event to send.</param>
        /// <param name="data">
        /// If <paramref name="eventFlags"/> is <see cref="MOUSEEVENTF.WHEEL"/>, then <paramref name="data"/> specifies the amount of wheel movement (positive value -> forward). One wheel click is <i>120</i> (use <see cref="MOUSEINPUT.CalculateWheelDeltaUInt32"/> to calculate wheel movement).<br/><br/>
        /// <b>Windows Vista</b>: If <paramref name="eventFlags"/> contains <see cref="MOUSEEVENTF.HWHEEL"/>, then <paramref name="data"/> specifies the amount of horizontal wheel movement (positive value -> right).<br/><br/>
        /// If <paramref name="eventFlags"/> does not contain <see cref="MOUSEEVENTF.WHEEL"/>, <see cref="MOUSEEVENTF.XDOWN"/>, or <see cref="MOUSEEVENTF.XUP"/>, then <paramref name="data"/> should be <i>0</i> (zero).<br/><br/>
        /// If <paramref name="eventFlags"/> contains <see cref="MOUSEEVENTF.XDOWN"/> or <see cref="MOUSEEVENTF.MOUSEEVENTF_XUP"/>, then <paramref name="data"/> specifies which X buttons (mouse side buttons) were pressed or released (<see cref="MouseEventData"/>).
        /// <paramref name="data"/> may be any combination of <see cref="MOUSEINPUT.XBUTTON1"/> and <see cref="MOUSEINPUT.XBUTTON2"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the input was successfully sent, otherwise <see langword="false"/>.
        /// </returns>
        public static bool SendMouseEvent(MOUSEEVENTF eventFlags, uint data = 0, int dx = 0, int dy = 0)
        {
            Span<INPUT> input = stackalloc INPUT[1];

            input[0].type = INPUT.TYPE.MOUSE;
            input[0].union.mi.dx = dx;
            input[0].union.mi.dy = dy;
            input[0].union.mi.mouseData = data;
            input[0].union.mi.dwFlags = eventFlags;

            return SendInput(input) == 1;
        }

        /// <summary>
        /// Synthesize a key event.
        /// </summary>
        /// <param name="eventFlags">Specifies various aspects of a keystroke. This can be certain combinations of <see cref="KEYEVENTF"/>.</param>
        /// <param name="scanCode">
        /// A hardware scan code for the key. If <paramref name="eventFlags"/> specifies <see cref="KEYEVENTF.UNICODE"/>, <paramref name="scanCode"/> specifies a Unicode character which is to be sent to the foreground application.<br/>
        /// This can be a <see cref="ScanCode"/> (enum), or the value returned by <see cref="MapVirtualKey"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the input was successfully sent, otherwise <see langword="false"/>.
        /// </returns>
        public static bool SendKeyEvent(KEYEVENTF eventFlags, ushort scanCode)
        {
            Span<INPUT> input = stackalloc INPUT[1];

            input[0].type = INPUT.TYPE.KEYBOARD;
            input[0].union.ki.wScan = scanCode;
            input[0].union.ki.dwFlags = eventFlags;

            return SendInput(input) == 1;
        }

        /// <summary>
        /// Retrieves the position of the mouse cursor, in screen coordinates.
        /// </summary>
        /// <returns>The screen coordinates of the cursor.</returns>
        public static unsafe POINT GetCursorPos()
        {
            if (!PInvoke.GetCursorPos(out POINT result))
            {
                throw new Win32Exception();
            }

            return result;
        }

        public static unsafe void SetWindowImmersiveDarkMode(nint hWnd, bool enabled)
        {
            const uint DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
            int isEnabled = enabled ? 1 : 0;
            HResult hr = DwmSetWindowAttribute(hWnd, DWMWA_USE_IMMERSIVE_DARK_MODE, &isEnabled, sizeof(int));
            if (hr != HResult.S_OK) throw new COMException(hr.ToString(), (int)hr);
        }

        public static unsafe int GetKeyboardLayoutListLength()
        {
            int length = PInvoke.GetKeyboardLayoutList(0, null);
            if (length == 0) throw new Win32Exception();
            return length;
        }

        public static unsafe void GetKeyboardLayoutList(Span<nint> buffer)
        {
            int retLength = -1;
            fixed (nint* pBuffer = buffer)
            {
                retLength = PInvoke.GetKeyboardLayoutList(buffer.Length, pBuffer);
            }

            if (retLength == 0 || retLength != buffer.Length) throw new Win32Exception();
        }

        /// <summary>
        /// Translates a character to the corresponding virtual-key code and shift state. The function translates the character by trying all installed input locale identifiers, starting with the current one.
        /// </summary>
        /// <returns>Same as <see cref="PInvoke.VkKeyScanEx(char, nint)"/>.</returns>
        public static short VkKeyScanAllSystemLayouts(char ch)
        {
            Span<nint> layouts = stackalloc nint[GetKeyboardLayoutListLength()];
            GetKeyboardLayoutList(layouts);

            nint currentLayout = PInvoke.GetKeyboardLayout(0);
            short result;

            result = PInvoke.VkKeyScanEx(ch, currentLayout);
            if (DidTranslationSucceed(result)) return result;

            foreach (var layout in layouts)
            {
                if (layout == currentLayout) continue;

                result = PInvoke.VkKeyScanEx(ch, layout);
                if (DidTranslationSucceed(result)) return result;
            }

            return -1;

            static bool DidTranslationSucceed(short translationResult)
            {
                sbyte low = (sbyte)(translationResult & 0xFF);
                sbyte high = (sbyte)((translationResult >> 8) & 0xFF);

                return low != -1 || high != -1;
            }
        }
    }
}
