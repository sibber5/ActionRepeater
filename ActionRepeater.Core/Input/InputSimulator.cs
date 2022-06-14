using System;
using ActionRepeater.Win32;
using ActionRepeater.Win32.Input;
using static ActionRepeater.Win32.PInvoke.Helpers;

namespace ActionRepeater.Core.Input;

public static class InputSimulator
{
    public enum MouseButton
    {
        /// <summary>
        /// The left button.
        /// </summary>
        Left,
        /// <summary>
        /// The middle button (wheel button)
        /// </summary>
        Middle,
        /// <summary>
        /// The right button.
        /// </summary>
        Right,
        /// <summary>
        /// The first X button (extra mouse [usually] side button)
        /// </summary>
        X1,
        /// <summary>
        /// The second X button (extra mouse [usually] side button)
        /// </summary>
        X2
    }

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
        (ushort)VirtualKey.PAUSE, // (BREAK/Ctrl+Esc) should map to scan code 70.
        (ushort)VirtualKey.SNAPSHOT,
    };

    /// <summary>
    /// Checks if <paramref name="key"/> is an extended key, and sets <paramref name="key"/> to the correct value for translation to scan code (if necessary).
    /// </summary>
    /// <param name="hasE1Prefix">
    /// Set to <typeparamref name="true"/> if the key is an extended key that has to be prefixed with 0xE1, otherwise set to <typeparamref name="false"/>
    /// which means - if the key is an extended key - that it has to be prefixed with 0xE0 (which <see cref="PInvoke.SendInput"/> will do autimatically if
    /// <see cref="KEYEVENTF.EXTENDEDKEY"/> is set).
    /// </param>
    /// <returns><typeparamref name="true"/> if key is extended key, otherwise <typeparamref name="false"/>.</returns>
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
    /// <param name="relative">If <typeparamref name="true"/>, move the cursor relative to its current position (by adding <paramref name="newPos"/>'s x and y values to the cursor's).</param>
    /// <param name="absolute">
    /// <para><b>(Will be ignored if <paramref name="relative"/> is <typeparamref name="true"/>)</b> Use normalized absolute coordinates between <i>0</i> (<see cref="ushort.MinValue"/>) and <i>65,535</i> (<see cref="ushort.MaxValue"/>).</para>
    /// <para><see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mouseinput#remarks">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <typeparamref name="true"/> if the input was successfully sent, otherwise <typeparamref name="false"/>.
    /// </returns>
    public static bool MoveMouse(POINT newPos, bool relative = false, bool absolute = false)
    {
        MOUSEEVENTF flags = MOUSEEVENTF.MOVE;
        if (!relative)
        {
            if (absolute)
            {
                flags = MOUSEEVENTF.MOVE | MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.VIRTUALDESK;
            }
            else
            {
                POINT curPos = GetCursorPos();
                newPos.x -= curPos.x;
                newPos.y -= curPos.y;
            }
        }

        return SendMouseEvent(
            eventFlags: flags,
            data: 0,
            dx: newPos.x,
            dy: newPos.y
        );
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
    /// <typeparamref name="true"/> if the input was successfully sent, otherwise <typeparamref name="false"/>.
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
    /// <typeparamref name="true"/> if the input was successfully sent, otherwise <typeparamref name="false"/>.
    /// </returns>
    public static bool SendMouseButtonDown(MouseButton button)
    {
        uint data = 0;
        MOUSEEVENTF flags = default;

        switch (button)
        {
            case MouseButton.X1:
                data = MOUSEINPUT.XBUTTON1;
                flags = MOUSEEVENTF.XDOWN;
                break;

            case MouseButton.X2:
                data = MOUSEINPUT.XBUTTON2;
                flags = MOUSEEVENTF.XDOWN;
                break;

            case MouseButton.Left:
                flags = MOUSEEVENTF.LEFTDOWN;
                break;

            case MouseButton.Right:
                flags = MOUSEEVENTF.RIGHTDOWN;
                break;

            case MouseButton.Middle:
                flags = MOUSEEVENTF.MIDDLEDOWN;
                break;
        }

        return SendMouseEvent(
            eventFlags: flags,
            data: data
        );
    }
    /// <summary>
    /// Moves mouse to the specified position, then synthesizes a button down event.
    /// </summary>
    /// <param name="pos">
    /// If <paramref name="relative"/> is <typeparamref name="false"/>, the position to move the mouse to.<br/>
    /// If <paramref name="relative"/> is <typeparamref name="true"/>, the distance to move the mouse from it's current position.
    /// </param>
    /// <param name="relative">If <typeparamref name="true"/>, move the cursor relative to its current position (by adding <paramref name="pos"/>'s x and y values to the cursor's).</param>
    /// <returns>
    /// <typeparamref name="true"/> if the input was successfully sent, otherwise <typeparamref name="false"/>.
    /// </returns>
    public static bool SendMouseButtonDown(MouseButton button, POINT pos, bool relative = false)
    {
        uint data = 0;
        MOUSEEVENTF flags = MOUSEEVENTF.MOVE;

        switch (button)
        {
            case MouseButton.X1:
                data = MOUSEINPUT.XBUTTON1;
                flags |= MOUSEEVENTF.XDOWN;
                break;

            case MouseButton.X2:
                data = MOUSEINPUT.XBUTTON2;
                flags |= MOUSEEVENTF.XDOWN;
                break;

            case MouseButton.Left:
                flags |= MOUSEEVENTF.LEFTDOWN;
                break;

            case MouseButton.Right:
                flags |= MOUSEEVENTF.RIGHTDOWN;
                break;

            case MouseButton.Middle:
                flags |= MOUSEEVENTF.MIDDLEDOWN;
                break;
        }

        if (!relative)
        {
            POINT curPos = GetCursorPos();
            pos.x -= curPos.x;
            pos.y -= curPos.y;
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
    /// <typeparamref name="true"/> if the input was successfully sent, otherwise <typeparamref name="false"/>.
    /// </returns>
    public static bool SendMouseButtonUp(MouseButton button)
    {
        uint data = 0;
        MOUSEEVENTF flags = default;

        switch (button)
        {
            case MouseButton.X1:
                data = MOUSEINPUT.XBUTTON1;
                flags = MOUSEEVENTF.XUP;
                break;

            case MouseButton.X2:
                data = MOUSEINPUT.XBUTTON2;
                flags = MOUSEEVENTF.XUP;
                break;

            case MouseButton.Left:
                flags = MOUSEEVENTF.LEFTUP;
                break;

            case MouseButton.Right:
                flags = MOUSEEVENTF.RIGHTUP;
                break;

            case MouseButton.Middle:
                flags = MOUSEEVENTF.MIDDLEUP;
                break;
        }

        return SendMouseEvent(
            eventFlags: flags,
            data: data
        );
    }
    /// <summary>
    /// Moves mouse to the specified position, then synthesizes a button up event.
    /// </summary>
    /// <param name="pos">
    /// If <paramref name="relative"/> is <typeparamref name="false"/>, the position to move the mouse to.<br/>
    /// If <paramref name="relative"/> is <typeparamref name="true"/>, the distance to move the mouse from it's current position.
    /// </param>
    /// <param name="relative">If <typeparamref name="true"/>, move the cursor relative to its current position (by adding <paramref name="pos"/>'s x and y values to the cursor's).</param>
    /// <returns>
    /// <typeparamref name="true"/> if the input was successfully sent, otherwise <typeparamref name="false"/>.
    /// </returns>
    public static bool SendMouseButtonUp(MouseButton button, POINT pos, bool relative = false)
    {
        uint data = 0;
        MOUSEEVENTF flags = MOUSEEVENTF.MOVE;

        switch (button)
        {
            case MouseButton.X1:
                data = MOUSEINPUT.XBUTTON1;
                flags |= MOUSEEVENTF.XUP;
                break;

            case MouseButton.X2:
                data = MOUSEINPUT.XBUTTON2;
                flags |= MOUSEEVENTF.XUP;
                break;

            case MouseButton.Left:
                flags |= MOUSEEVENTF.LEFTUP;
                break;

            case MouseButton.Right:
                flags |= MOUSEEVENTF.RIGHTUP;
                break;

            case MouseButton.Middle:
                flags |= MOUSEEVENTF.MIDDLEUP;
                break;
        }

        if (!relative)
        {
            POINT curPos = GetCursorPos();
            pos.x -= curPos.x;
            pos.y -= curPos.y;
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
    /// <typeparamref name="true"/> if the inputs were successfully sent, otherwise <typeparamref name="false"/>.
    /// </returns>
    public static bool SendMouseButtonClick(MouseButton button)
    {
        uint data = 0;
        MOUSEEVENTF flags0 = default;
        MOUSEEVENTF flags1 = default;

        switch (button)
        {
            case MouseButton.X1:
                data = MOUSEINPUT.XBUTTON1;
                flags0 = MOUSEEVENTF.XDOWN;
                flags1 = MOUSEEVENTF.XUP;
                break;

            case MouseButton.X2:
                data = MOUSEINPUT.XBUTTON2;
                flags0 = MOUSEEVENTF.XDOWN;
                flags1 = MOUSEEVENTF.XUP;
                break;

            case MouseButton.Left:
                flags0 = MOUSEEVENTF.LEFTDOWN;
                flags1 = MOUSEEVENTF.LEFTUP;
                break;

            case MouseButton.Right:
                flags0 = MOUSEEVENTF.RIGHTDOWN;
                flags1 = MOUSEEVENTF.RIGHTUP;
                break;

            case MouseButton.Middle:
                flags0 = MOUSEEVENTF.MIDDLEDOWN;
                flags1 = MOUSEEVENTF.MIDDLEUP;
                break;
        }

        INPUT[] input = new[]
        {
            new INPUT
            {
                type = INPUT.TYPE.MOUSE,
                union = new INPUT.UNION
                {
                    mi = new MOUSEINPUT
                    {
                        mouseData = data,
                        dwFlags = flags0,
                        time = 0
                    }
                }
            },
            new INPUT
            {
                type = INPUT.TYPE.MOUSE,
                union = new INPUT.UNION
                {
                    mi = new MOUSEINPUT
                    {
                        mouseData = data,
                        dwFlags = flags1,
                        time = 0
                    }
                }
            },
        };

        return PInvoke.SendInput(input) == 2;
    }
    /// <summary>
    /// Moves mouse to the specified position, then synthesizes a button down event followed by a button up event.
    /// </summary>
    /// <param name="pos">
    /// If <paramref name="relative"/> is <typeparamref name="false"/>, the position to move the mouse to.<br/>
    /// If <paramref name="relative"/> is <typeparamref name="true"/>, the distance to move the mouse from it's current position.
    /// </param>
    /// <param name="relative">If <typeparamref name="true"/>, move the cursor relative to its current position (by adding <paramref name="pos"/>'s x and y values to the cursor's).</param>
    /// <returns>
    /// <typeparamref name="true"/> if the inputs were successfully sent, otherwise <typeparamref name="false"/>.
    /// </returns>
    public static bool SendMouseButtonClick(MouseButton button, POINT pos, bool relative = false)
    {
        uint data = 0;
        MOUSEEVENTF flags0 = MOUSEEVENTF.MOVE;
        MOUSEEVENTF flags1 = default;

        switch (button)
        {
            case MouseButton.X1:
                data = MOUSEINPUT.XBUTTON1;
                flags0 |= MOUSEEVENTF.XDOWN;
                flags1 = MOUSEEVENTF.XUP;
                break;

            case MouseButton.X2:
                data = MOUSEINPUT.XBUTTON2;
                flags0 |= MOUSEEVENTF.XDOWN;
                flags1 = MOUSEEVENTF.XUP;
                break;

            case MouseButton.Left:
                flags0 |= MOUSEEVENTF.LEFTDOWN;
                flags1 = MOUSEEVENTF.LEFTUP;
                break;

            case MouseButton.Right:
                flags0 |= MOUSEEVENTF.RIGHTDOWN;
                flags1 = MOUSEEVENTF.RIGHTUP;
                break;

            case MouseButton.Middle:
                flags0 |= MOUSEEVENTF.MIDDLEDOWN;
                flags1 = MOUSEEVENTF.MIDDLEUP;
                break;
        }

        if (!relative)
        {
            POINT curPos = GetCursorPos();
            pos.x -= curPos.x;
            pos.y -= curPos.y;
        }

        INPUT[] input = new[]
        {
            new INPUT
            {
                type = INPUT.TYPE.MOUSE,
                union = new INPUT.UNION
                {
                    mi = new MOUSEINPUT
                    {
                        dx = pos.x,
                        dy = pos.y,
                        mouseData = data,
                        dwFlags = flags0,
                        time = 0
                    }
                }
            },
            new INPUT
            {
                type = INPUT.TYPE.MOUSE,
                union = new INPUT.UNION
                {
                    mi = new MOUSEINPUT
                    {
                        mouseData = data,
                        dwFlags = flags1,
                        time = 0
                    }
                }
            },
        };

        return PInvoke.SendInput(input) == 2;
    }

    /// <summary>
    /// Synthesize a key down event.
    /// </summary>
    /// <param name="key">The Virtual Key code of the key to send. (will be mapped to the scan code with the extended key flag if necessary)</param>
    /// <param name="forceExtendedKey">Force the extended key flag. should only be used when sending the enter key on the numpad.</param>
    /// <returns>
    /// <typeparamref name="true"/> if the input was successfully sent, otherwise <typeparamref name="false"/>.
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

            INPUT[] input = new[]
            {
                new INPUT
                {
                    type = INPUT.TYPE.KEYBOARD,
                    union = new INPUT.UNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wScan = 0xE1,
                            dwFlags = KEYEVENTF.SCANCODE,
                            time = 0
                        }
                    }
                },
                new INPUT
                {
                    type = INPUT.TYPE.KEYBOARD,
                    union = new INPUT.UNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wScan = scanCode,
                            dwFlags = KEYEVENTF.SCANCODE,
                            time = 0
                        }
                    }
                }
            };

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
    /// <typeparamref name="true"/> if the input was successfully sent, otherwise <typeparamref name="false"/>.
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

            INPUT[] input = new[]
            {
                new INPUT
                {
                    type = INPUT.TYPE.KEYBOARD,
                    union = new INPUT.UNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wScan = 0xE1,
                            dwFlags = KEYEVENTF.KEYUP | KEYEVENTF.SCANCODE,
                            time = 0
                        }
                    }
                },
                new INPUT
                {
                    type = INPUT.TYPE.KEYBOARD,
                    union = new INPUT.UNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wScan = scanCode,
                            dwFlags = KEYEVENTF.KEYUP | KEYEVENTF.SCANCODE,
                            time = 0
                        }
                    }
                }
            };

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
    /// <typeparamref name="true"/> if the inputs were successfully sent, otherwise <typeparamref name="false"/>.
    /// </returns>
    public static bool SendKeyPress(VirtualKey key, bool forceExtendedKey = false)
    {
        ushort vkCode = (ushort)key;
        ushort scanCode = (ushort)PInvoke.MapVirtualKey(vkCode, VirtualKeyMapType.VK_TO_VSC_EX);

        bool hasE1Prefix = false;
        bool useExtendedKey = forceExtendedKey || IsExtendedKey(ref vkCode, out hasE1Prefix);

        if (hasE1Prefix)
        {
            INPUT[] inputE1 = new[]
            {
                new INPUT
                {
                    type = INPUT.TYPE.KEYBOARD,
                    union = new INPUT.UNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wScan = 0xE1,
                            dwFlags = KEYEVENTF.SCANCODE,
                            time = 0
                        }
                    }
                },
                new INPUT
                {
                    type = INPUT.TYPE.KEYBOARD,
                    union = new INPUT.UNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wScan = scanCode,
                            dwFlags = KEYEVENTF.SCANCODE,
                            time = 0
                        }
                    }
                },
                new INPUT
                {
                    type = INPUT.TYPE.KEYBOARD,
                    union = new INPUT.UNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wScan = 0xE1,
                            dwFlags = KEYEVENTF.KEYUP | KEYEVENTF.SCANCODE,
                            time = 0
                        }
                    }
                },
                new INPUT
                {
                    type = INPUT.TYPE.KEYBOARD,
                    union = new INPUT.UNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wScan = scanCode,
                            dwFlags = KEYEVENTF.KEYUP | KEYEVENTF.SCANCODE,
                            time = 0
                        }
                    }
                }
            };

            return PInvoke.SendInput(inputE1) == 2;
        }

        KEYEVENTF eventFlags = KEYEVENTF.SCANCODE | (useExtendedKey ? KEYEVENTF.EXTENDEDKEY : 0);
        INPUT[] input = new[]
        {
            new INPUT
            {
                type = INPUT.TYPE.KEYBOARD,
                union = new INPUT.UNION
                {
                    ki = new KEYBDINPUT
                    {
                        wScan = scanCode,
                        dwFlags = eventFlags,
                        time = 0
                    }
                }
            },
            new INPUT
            {
                type = INPUT.TYPE.KEYBOARD,
                union = new INPUT.UNION
                {
                    ki = new KEYBDINPUT
                    {
                        wScan = scanCode,
                        dwFlags = eventFlags | KEYEVENTF.KEYUP,
                        time = 0
                    }
                }
            }
        };

        return PInvoke.SendInput(input) == 2;
    }
}
