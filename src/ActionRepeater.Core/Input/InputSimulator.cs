using System;
using ActionRepeater.Win32;
using ActionRepeater.Win32.Input;
using ActionRepeater.Win32.Utilities;
using static ActionRepeater.Win32.PInvoke.Helpers;

namespace ActionRepeater.Core.Input;

public static class InputSimulator
{
    /// <summary>
    /// Extended keys which are preceded by 0xE0. This array is sorted.
    /// </summary>
    private static readonly ushort[] _exKeysE0 = new[]
    {
        // not always extended: VK_RETURN, only if intended key is numpad enter.
        (ushort)VirtualKey.PRIOR,
        (ushort)VirtualKey.NEXT,
        (ushort)VirtualKey.END,
        (ushort)VirtualKey.HOME,
        (ushort)VirtualKey.LEFT,
        (ushort)VirtualKey.UP,
        (ushort)VirtualKey.RIGHT,
        (ushort)VirtualKey.DOWN,
        (ushort)VirtualKey.INSERT,
        (ushort)VirtualKey.DELETE,
        (ushort)VirtualKey.LWIN, // NOT extended key according to docs, but doesnt work without extended key flag.
        (ushort)VirtualKey.RWIN, // NOT extended key according to docs, but doesnt work without extended key flag.
        (ushort)VirtualKey.DIVIDE,
        (ushort)VirtualKey.NUMLOCK,
        (ushort)VirtualKey.RCONTROL,
        (ushort)VirtualKey.RMENU,
    };

    /// <summary>
    /// Extended keys which are preceded by 0xE1.
    /// </summary>
    private static readonly ushort[] _exKeysE1 = new[]
    {
        (ushort)VirtualKey.PAUSE, // (BREAK/Ctrl+Esc) should map to scan code 0x46 (70).
        (ushort)VirtualKey.SNAPSHOT,
    };

    /// <summary>
    /// Checks if <paramref name="key"/> is an extended key, and sets <paramref name="key"/> to the correct value for translation to scan code (if necessary).
    /// </summary>
    /// <param name="hasE1Prefix">
    /// Set to <see langword="true"/> if the key is an extended key that has to be prefixed with 0xE1, otherwise set to <see langword="false"/>
    /// which means - if the key is an extended key - that it has to be prefixed with 0xE0 (which <see cref="PInvoke.SendInput"/> will do autimatically if
    /// <see cref="KEYEVENTF.EXTENDEDKEY"/> is set).
    /// </param>
    /// <returns><see langword="true"/> if key is extended key, otherwise <see langword="false"/>.</returns>
    private static bool IsExtendedKey(ref ushort key, out bool hasE1Prefix)
    {
        bool useExtendedKey;
        if (_exKeysE1[0] == key || _exKeysE1[1] == key)
        {
            hasE1Prefix = true;
            useExtendedKey = true;

            // PAUSE (or BREAK which is Ctrl+Esc) is an extended key. it should map to ScanCode.CANCEL/SCROLL but instead maps to 0.
            // VK_SCROLL maps to the correct scan code.
            if (useExtendedKey && key == (ushort)VirtualKey.PAUSE)
            {
                key = (ushort)VirtualKey.SCROLL;
            }
        }
        else
        {
            hasE1Prefix = false;
            int extendedKeyIndex = Array.BinarySearch(_exKeysE0, key);
            useExtendedKey = extendedKeyIndex >= 0;
        }

        return useExtendedKey;
    }

    /// <summary>
    /// Moves the mouse cursor to the specified position.
    /// </summary>
    /// <param name="relativePos">If <see langword="true"/>, move the cursor relative to its current position (by adding <paramref name="newPos"/>'s x and y values to the cursor's).</param>
    /// <param name="absoluteCoords">
    /// <para><b>(Will be ignored if <paramref name="relativePos"/> is <see langword="true"/>)</b> Use normalized absolute coordinates between <c>0</c> (<see cref="ushort.MinValue"/>) and <c>65,535</c> (<see cref="ushort.MaxValue"/>).</para>
    /// <para><see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mouseinput#remarks">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="convertToAbsolute">
    /// <para>
    /// Convert <paramref name="newPos"/> to absolute coordinates if <paramref name="relativePos"/> is <see langword="false"/>,
    /// in order to avoid windows messing with the movement (<see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mouseinput#remarks">click for more info - last 2 paragraphs</see>).
    /// </para>
    /// <para>
    /// Will be ignored if <paramref name="convertToAbsolute"/> is <see langword="true"/>.
    /// </para>
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input was successfully sent, otherwise <see langword="false"/>.
    /// </returns>
    public static bool MoveMouse(POINT newPos, bool relativePos = false, bool absoluteCoords = false, bool convertToAbsolute = true)
    {
        MOUSEEVENTF flags = MOUSEEVENTF.MOVE;

        if (relativePos) return SendMouseEvent(flags, 0, newPos.x, newPos.y);
        
        if (absoluteCoords)
        {
            flags = MOUSEEVENTF.MOVE | MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.VIRTUALDESK;
            return SendMouseEvent(flags, 0, newPos.x, newPos.y);
        }

        if (convertToAbsolute)
        {
            flags = MOUSEEVENTF.MOVE | MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.VIRTUALDESK;

            // Convert to absolute coordinates because because "Relative mouse motion is subject to the effects of the mouse speed and the two-mouse threshold values."
            // see last 2 paragraphs: https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mouseinput#remarks
            POINT absolutePos = ScreenCoordsConverter.GetAbsoluteCoordinateFromPosRelToPrimary(newPos);
            newPos = absolutePos;
        }
        else
        {
            POINT curPos = GetCursorPos();
            newPos.x -= curPos.x;
            newPos.y -= curPos.y;
        }

        return SendMouseEvent(flags, 0, newPos.x, newPos.y);
    }

    /// <summary>
    /// Synthesize mouse wheel movement/clicks.
    /// </summary>
    /// <param name="clickCount">
    /// The number of wheel clicks.<br/>
    /// A positive value indicates that the wheel was rotated forward/to the right, away from the user;
    /// a negative value indicates that the wheel was rotated backward/to the left, toward the user.
    /// </param>
    /// <param name="horizontalWheelMovement"><b>Windows XP/2000</b>: This value is not supported.</param>
    /// <returns>
    /// <see langword="true"/> if the input was successfully sent, otherwise <see langword="false"/>.
    /// </returns>
    public static bool MoveMouseWheel(int clickCount, bool horizontalWheelMovement = false)
    {
        return SendMouseEvent(
            eventFlags: horizontalWheelMovement ? MOUSEEVENTF.HWHEEL : MOUSEEVENTF.WHEEL,
            data: MOUSEINPUT.CalculateWheelDeltaUInt32(clickCount)
        );
    }

    /// <summary>Synthesizes a button down event.</summary>
    /// <returns>
    /// <see langword="true"/> if the input was successfully sent, otherwise <see langword="false"/>.
    /// </returns>
    public static bool SendMouseButtonDown(MouseButton button)
    {
        (MOUSEEVENTF flags, uint data) = button switch
        {
            MouseButton.X1     => (MOUSEEVENTF.XDOWN, MOUSEINPUT.XBUTTON1),
            MouseButton.X2     => (MOUSEEVENTF.XDOWN, MOUSEINPUT.XBUTTON2),
            MouseButton.Left   => (MOUSEEVENTF.LEFTDOWN, 0u),
            MouseButton.Right  => (MOUSEEVENTF.RIGHTDOWN, 0u),
            MouseButton.Middle => (MOUSEEVENTF.MIDDLEDOWN, 0u),
            _ => throw new NotSupportedException()
        };

        return SendMouseEvent(
            eventFlags: flags,
            data: data
        );
    }
    /// <summary>
    /// Moves mouse to the specified position, then synthesizes a button down event.
    /// </summary>
    /// <param name="pos">
    /// If <paramref name="relative"/> is <see langword="false"/>, the position to move the mouse to.<br/>
    /// If <paramref name="relative"/> is <see langword="true"/>, the distance to move the mouse from it's current position.
    /// </param>
    /// <param name="relative">If <see langword="true"/>, move the cursor relative to its current position (by adding <paramref name="pos"/>'s x and y values to the cursor's).</param>
    /// <returns>
    /// <see langword="true"/> if the input was successfully sent, otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// if <paramref name="relative"/> is <see langword="false"/>, <paramref name="pos"/> specifies the position relative the the primary monitors top left corner (The primary monitor's top left corner is (0, 0)).
    /// </remarks>
    public static bool SendMouseButtonDown(MouseButton button, POINT pos, bool relative = false)
    {
        (MOUSEEVENTF flags, uint data) = button switch
        {
            MouseButton.X1     => (MOUSEEVENTF.MOVE | MOUSEEVENTF.XDOWN, MOUSEINPUT.XBUTTON1),
            MouseButton.X2     => (MOUSEEVENTF.MOVE | MOUSEEVENTF.XDOWN, MOUSEINPUT.XBUTTON2),
            MouseButton.Left   => (MOUSEEVENTF.MOVE | MOUSEEVENTF.LEFTDOWN, 0u),
            MouseButton.Right  => (MOUSEEVENTF.MOVE | MOUSEEVENTF.RIGHTDOWN, 0u),
            MouseButton.Middle => (MOUSEEVENTF.MOVE | MOUSEEVENTF.MIDDLEDOWN, 0u),
            _ => throw new NotSupportedException()
        };

        if (!relative)
        {
            flags |= MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.VIRTUALDESK;

            POINT absolutePos = ScreenCoordsConverter.GetAbsoluteCoordinateFromPosRelToPrimary(pos);
            pos = absolutePos;
        }

        return SendMouseEvent(
            eventFlags: flags,
            data: data,
            dx: pos.x,
            dy: pos.y
        );
    }

    /// <summary>Synthesizes a button up event.</summary>
    /// <returns>
    /// <see langword="true"/> if the input was successfully sent, otherwise <see langword="false"/>.
    /// </returns>
    public static bool SendMouseButtonUp(MouseButton button)
    {
        (MOUSEEVENTF flags, uint data) = button switch
        {
            MouseButton.X1     => (MOUSEEVENTF.XUP, MOUSEINPUT.XBUTTON1),
            MouseButton.X2     => (MOUSEEVENTF.XUP, MOUSEINPUT.XBUTTON2),
            MouseButton.Left   => (MOUSEEVENTF.LEFTUP, 0u),
            MouseButton.Right  => (MOUSEEVENTF.RIGHTUP, 0u),
            MouseButton.Middle => (MOUSEEVENTF.MIDDLEUP, 0u),
            _ => throw new NotSupportedException()
        };

        return SendMouseEvent(
            eventFlags: flags,
            data: data
        );
    }
    /// <summary>
    /// Moves mouse to the specified position, then synthesizes a button up event.
    /// </summary>
    /// <param name="pos">
    /// If <paramref name="relative"/> is <see langword="false"/>, the position to move the mouse to.<br/>
    /// If <paramref name="relative"/> is <see langword="true"/>, the distance to move the mouse from it's current position.
    /// </param>
    /// <param name="relative">If <see langword="true"/>, move the cursor relative to its current position (by adding <paramref name="pos"/>'s x and y values to the cursor's).</param>
    /// <returns>
    /// <see langword="true"/> if the input was successfully sent, otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// if <paramref name="relative"/> is <see langword="false"/>, <paramref name="pos"/> specifies the position relative the the primary monitors top left corner (The primary monitor's top left corner is (0, 0)).
    /// </remarks>
    public static bool SendMouseButtonUp(MouseButton button, POINT pos, bool relative = false)
    {
        (MOUSEEVENTF flags, uint data) = button switch
        {
            MouseButton.X1     => (MOUSEEVENTF.MOVE | MOUSEEVENTF.XUP, MOUSEINPUT.XBUTTON1),
            MouseButton.X2     => (MOUSEEVENTF.MOVE | MOUSEEVENTF.XUP, MOUSEINPUT.XBUTTON2),
            MouseButton.Left   => (MOUSEEVENTF.MOVE | MOUSEEVENTF.LEFTUP, 0u),
            MouseButton.Right  => (MOUSEEVENTF.MOVE | MOUSEEVENTF.RIGHTUP, 0u),
            MouseButton.Middle => (MOUSEEVENTF.MOVE | MOUSEEVENTF.MIDDLEUP, 0u),
            _ => throw new NotSupportedException()
        };

        if (!relative)
        {
            flags |= MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.VIRTUALDESK;

            POINT absolutePos = ScreenCoordsConverter.GetAbsoluteCoordinateFromPosRelToPrimary(pos);
            pos = absolutePos;
        }

        return SendMouseEvent(
            eventFlags: flags,
            data: data,
            dx: pos.x,
            dy: pos.y
        );
    }

    /// <summary>Synthesizes a button down event followed by a button up event.</summary>
    /// <returns>
    /// <see langword="true"/> if the inputs were successfully sent, otherwise <see langword="false"/>.
    /// </returns>
    public static bool SendMouseButtonClick(MouseButton button)
    {
        (MOUSEEVENTF flags0, MOUSEEVENTF flags1, uint data) = button switch
        {
            MouseButton.X1     => (MOUSEEVENTF.XDOWN, MOUSEEVENTF.XUP, MOUSEINPUT.XBUTTON1),
            MouseButton.X2     => (MOUSEEVENTF.XDOWN, MOUSEEVENTF.XUP, MOUSEINPUT.XBUTTON2),
            MouseButton.Left   => (MOUSEEVENTF.LEFTDOWN, MOUSEEVENTF.LEFTUP, 0u),
            MouseButton.Right  => (MOUSEEVENTF.RIGHTDOWN, MOUSEEVENTF.RIGHTUP, 0u),
            MouseButton.Middle => (MOUSEEVENTF.MIDDLEDOWN, MOUSEEVENTF.MIDDLEUP, 0u),
            _ => throw new NotSupportedException()
        };

        Span<INPUT> input = stackalloc INPUT[2];

        input[0].type = INPUT.TYPE.MOUSE;
        input[0].union.mi.mouseData = data;
        input[0].union.mi.dwFlags = flags0;

        input[1].type = INPUT.TYPE.MOUSE;
        input[1].union.mi.mouseData = data;
        input[1].union.mi.dwFlags = flags1;

        return PInvoke.SendInput(input) == 2;
    }
    /// <summary>
    /// Moves mouse to the specified position, then synthesizes a button down event followed by a button up event.
    /// </summary>
    /// <param name="pos">
    /// If <paramref name="relative"/> is <see langword="false"/>, the position to move the mouse to.<br/>
    /// If <paramref name="relative"/> is <see langword="true"/>, the distance to move the mouse from it's current position.
    /// </param>
    /// <param name="relative">If <see langword="true"/>, move the cursor relative to its current position (by adding <paramref name="pos"/>'s x and y values to the cursor's).</param>
    /// <returns>
    /// <see langword="true"/> if the inputs were successfully sent, otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// if <paramref name="relative"/> is <see langword="false"/>, <paramref name="pos"/> specifies the position relative the the primary monitors top left corner (The primary monitor's top left corner is (0, 0)).
    /// </remarks>
    public static bool SendMouseButtonClick(MouseButton button, POINT pos, bool relative = false)
    {
        (MOUSEEVENTF flags0, MOUSEEVENTF flags1, uint data) = button switch
        {
            MouseButton.X1     => (MOUSEEVENTF.MOVE | MOUSEEVENTF.XDOWN, MOUSEEVENTF.XUP, MOUSEINPUT.XBUTTON1),
            MouseButton.X2     => (MOUSEEVENTF.MOVE | MOUSEEVENTF.XDOWN, MOUSEEVENTF.XUP, MOUSEINPUT.XBUTTON2),
            MouseButton.Left   => (MOUSEEVENTF.MOVE | MOUSEEVENTF.LEFTDOWN, MOUSEEVENTF.LEFTUP, 0u),
            MouseButton.Right  => (MOUSEEVENTF.MOVE | MOUSEEVENTF.RIGHTDOWN, MOUSEEVENTF.RIGHTUP, 0u),
            MouseButton.Middle => (MOUSEEVENTF.MOVE | MOUSEEVENTF.MIDDLEDOWN, MOUSEEVENTF.MIDDLEUP, 0u),
            _ => throw new NotSupportedException()
        };

        if (!relative)
        {
            flags0 |= MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.VIRTUALDESK;
            
            POINT absolutePos = ScreenCoordsConverter.GetAbsoluteCoordinateFromPosRelToPrimary(pos);
            pos = absolutePos;
        }

        Span<INPUT> input = stackalloc INPUT[2];

        input[0].type = INPUT.TYPE.MOUSE;
        input[0].union.mi.dx = pos.x;
        input[0].union.mi.dy = pos.y;
        input[0].union.mi.mouseData = data;
        input[0].union.mi.dwFlags = flags0;

        input[1].type = INPUT.TYPE.MOUSE;
        input[1].union.mi.mouseData = data;
        input[1].union.mi.dwFlags = flags1;

        return PInvoke.SendInput(input) == 2;
    }

    /// <summary>
    /// Synthesize a key down event.
    /// </summary>
    /// <param name="key">The Virtual Key code of the key to send. (will be mapped to the scan code with the extended key flag if necessary)</param>
    /// <param name="forceExtendedKey">Force the extended key flag. should only be used when sending the enter key on the numpad.</param>
    /// <returns>
    /// <see langword="true"/> if the input was successfully sent, otherwise <see langword="false"/>.
    /// </returns>
    public static bool SendKeyDown(VirtualKey key, bool forceExtendedKey = false)
    {
        ushort vkCode = (ushort)key;
        ushort scanCode = (ushort)PInvoke.MapVirtualKey(vkCode, VirtualKeyMapType.VK_TO_VSC_EX);

        bool hasE1Prefix = false;
        if (forceExtendedKey || IsExtendedKey(ref vkCode, out hasE1Prefix))
        {
            if (!hasE1Prefix)
            {
                return SendKeyEvent(KEYEVENTF.SCANCODE | KEYEVENTF.EXTENDEDKEY, scanCode);
            }

            Span<INPUT> input = stackalloc INPUT[2];

            input[0].type = INPUT.TYPE.KEYBOARD;
            input[0].union.ki.wScan = 0xE1;
            input[0].union.ki.dwFlags = KEYEVENTF.SCANCODE;

            input[1].type = INPUT.TYPE.KEYBOARD;
            input[1].union.ki.wScan = scanCode;
            input[1].union.ki.dwFlags = KEYEVENTF.SCANCODE;

            return PInvoke.SendInput(input) == 2;
        }

        return SendKeyEvent(KEYEVENTF.SCANCODE, scanCode);
    }

    /// <summary>
    /// Synthesize a key up event.
    /// </summary>
    /// <param name="key">The Virtual Key code of the key to send. (will be mapped to the scan code with the extended key flag if necessary)</param>
    /// <param name="forceExtendedKey">Force the extended key flag. should only be used when sending the enter key on the numpad.</param>
    /// <returns>
    /// <see langword="true"/> if the input was successfully sent, otherwise <see langword="false"/>.
    /// </returns>
    public static bool SendKeyUp(VirtualKey key, bool forceExtendedKey = false)
    {
        ushort vkCode = (ushort)key;
        ushort scanCode = (ushort)PInvoke.MapVirtualKey(vkCode, VirtualKeyMapType.VK_TO_VSC_EX);

        bool hasE1Prefix = false;
        if (forceExtendedKey || IsExtendedKey(ref vkCode, out hasE1Prefix))
        {
            if (!hasE1Prefix)
            {
                return SendKeyEvent(KEYEVENTF.KEYUP | KEYEVENTF.SCANCODE | KEYEVENTF.EXTENDEDKEY, scanCode);
            }

            Span<INPUT> input = stackalloc INPUT[2];

            input[0].type = INPUT.TYPE.KEYBOARD;
            input[0].union.ki.wScan = 0xE1;
            input[0].union.ki.dwFlags = KEYEVENTF.KEYUP | KEYEVENTF.SCANCODE;

            input[1].type = INPUT.TYPE.KEYBOARD;
            input[1].union.ki.wScan = scanCode;
            input[1].union.ki.dwFlags = KEYEVENTF.KEYUP | KEYEVENTF.SCANCODE;

            return PInvoke.SendInput(input) == 2;
        }

        return SendKeyEvent(KEYEVENTF.KEYUP | KEYEVENTF.SCANCODE, scanCode);
    }

    /// <summary>
    /// Synthesize a key down event followed by a key up event with the same key.
    /// </summary>
    /// <param name="key">The Virtual Key code of the key to send. (will be mapped to the scan code with the extended key flag if necessary)</param>
    /// <param name="forceExtendedKey">Force the extended key flag. should only be used when sending the enter key on the numpad.</param>
    /// <returns>
    /// <see langword="true"/> if the inputs were successfully sent, otherwise <see langword="false"/>.
    /// </returns>
    public static bool SendKeyPress(VirtualKey key, bool forceExtendedKey = false)
    {
        ushort vkCode = (ushort)key;
        ushort scanCode = (ushort)PInvoke.MapVirtualKey(vkCode, VirtualKeyMapType.VK_TO_VSC_EX);

        bool hasE1Prefix = false;
        bool useExtendedKey = forceExtendedKey || IsExtendedKey(ref vkCode, out hasE1Prefix);

        if (hasE1Prefix)
        {
            Span<INPUT> inputE1 = stackalloc INPUT[4];

            inputE1[0].type = INPUT.TYPE.KEYBOARD;
            inputE1[0].union.ki.wScan = 0xE1;
            inputE1[0].union.ki.dwFlags = KEYEVENTF.SCANCODE;

            inputE1[1].type = INPUT.TYPE.KEYBOARD;
            inputE1[1].union.ki.wScan = scanCode;
            inputE1[1].union.ki.dwFlags = KEYEVENTF.SCANCODE;

            inputE1[2].type = INPUT.TYPE.KEYBOARD;
            inputE1[2].union.ki.wScan = 0xE1;
            inputE1[2].union.ki.dwFlags = KEYEVENTF.KEYUP | KEYEVENTF.SCANCODE;

            inputE1[3].type = INPUT.TYPE.KEYBOARD;
            inputE1[3].union.ki.wScan = scanCode;
            inputE1[3].union.ki.dwFlags = KEYEVENTF.KEYUP | KEYEVENTF.SCANCODE;

            return PInvoke.SendInput(inputE1) == 2;
        }

        KEYEVENTF eventFlags = KEYEVENTF.SCANCODE | (useExtendedKey ? KEYEVENTF.EXTENDEDKEY : 0);

        Span<INPUT> input = stackalloc INPUT[2];

        input[0].type = INPUT.TYPE.KEYBOARD;
        input[0].union.ki.wScan = scanCode;
        input[0].union.ki.dwFlags = eventFlags;

        input[1].type = INPUT.TYPE.KEYBOARD;
        input[1].union.ki.wScan = scanCode;
        input[1].union.ki.dwFlags = KEYEVENTF.KEYUP | eventFlags;

        return PInvoke.SendInput(input) == 2;
    }
}
