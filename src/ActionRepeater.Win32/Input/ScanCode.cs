namespace ActionRepeater.Win32.Input;

// Taken from https://github.com/dotnet/pinvoke
// MIT License
// 
// Copyright(c) .NET Foundation and Contributors
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
/// General keyboard scan code constants on the same order that it can be found on PInvoke.User32.VirtualKey constants.
/// </summary>
/// <remarks>Scan codes are device-dependant values, these are general values used by most keyboards.</remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "This is intentional (some virtual keys have the same scan code).")]
public enum ScanCode : ushort
{
    NONAME = 0,
    ESCAPE = 1,
    KEY_1 = 2,
    KEY_2 = 3,
    KEY_3 = 4,
    KEY_4 = 5,
    KEY_5 = 6,
    KEY_6 = 7,
    KEY_7 = 8,
    KEY_8 = 9,
    KEY_9 = 10,
    KEY_0 = 11,
    OEM_MINUS = 12,
    OEM_PLUS = 13,
    BACK = 14,
    TAB = 0xF,

    /// <summary>Same as <see cref="MEDIA_PREV_TRACK"/></summary>
    KEY_Q = 0x10,
    /// <summary>Same as <see cref="KEY_Q"/></summary>
    MEDIA_PREV_TRACK = 0x10,

    KEY_W = 17,
    KEY_E = 18,
    KEY_R = 19,
    KEY_T = 20,
    KEY_Y = 21,
    KEY_U = 22,
    KEY_I = 23,
    KEY_O = 24,

    /// <summary>Same as <see cref="MEDIA_NEXT_TRACK"/></summary>
    KEY_P = 25,
    /// <summary>Same as <see cref="KEY_P"/></summary>
    MEDIA_NEXT_TRACK = 25,

    OEM_4 = 26,
    OEM_6 = 27,
    RETURN = 28,

    /// <summary>Same as <see cref="LCONTROL"/> and <see cref="RCONTROL"/></summary>
    CONTROL = 29,
    /// <summary>Same as <see cref="CONTROL"/></summary>
    LCONTROL = 29,
    /// <summary>Same as <see cref="CONTROL"/></summary>
    RCONTROL = 29,

    KEY_A = 30,
    KEY_S = 0x1F,

    /// <summary>Same as <see cref="VOLUME_MUTE"/></summary>
    KEY_D = 0x20,
    /// <summary>Same as <see cref="KEY_D"/></summary>
    VOLUME_MUTE = 0x20,

    /// <summary>Same as <see cref="LAUNCH_APP2"/></summary>
    KEY_F = 33,
    /// <summary>Same as <see cref="KEY_F"/></summary>
    LAUNCH_APP2 = 33,

    /// <summary>Same as <see cref="MEDIA_PLAY_PAUSE"/></summary>
    KEY_G = 34,
    /// <summary>Same as <see cref="KEY_G"/></summary>
    MEDIA_PLAY_PAUSE = 34,

    KEY_H = 35,

    /// <summary>Same as <see cref="MEDIA_STOP"/></summary>
    KEY_J = 36,
    /// <summary>Same as <see cref="KEY_J"/></summary>
    MEDIA_STOP = 36,

    KEY_K = 37,
    KEY_L = 38,
    OEM_1 = 39,
    OEM_7 = 40,
    OEM_3 = 41,

    /// <summary>Same as <see cref="LSHIFT"/>, but NOT same as <see cref="RSHIFT"/></summary>
    SHIFT = 42,
    /// <summary>Same as <see cref="SHIFT"/>, but NOT same as <see cref="RSHIFT"/></summary>
    LSHIFT = 42,

    OEM_5 = 43,
    KEY_Z = 44,
    KEY_X = 45,

    /// <summary>Same as <see cref="VOLUME_DOWN"/></summary>
    KEY_C = 46,
    /// <summary>Same as <see cref="KEY_C"/></summary>
    VOLUME_DOWN = 46,

    KEY_V = 47,

    /// <summary>Same as <see cref="VOLUME_UP"/></summary>
    KEY_B = 48,
    /// <summary>Same as <see cref="KEY_B"/></summary>
    VOLUME_UP = 48,

    KEY_N = 49,

    /// <summary>Same as <see cref="BROWSER_HOME"/></summary>
    KEY_M = 50,
    /// <summary>Same as <see cref="KEY_M"/></summary>
    BROWSER_HOME = 50,

    OEM_COMMA = 51,
    OEM_PERIOD = 52,

    /// <summary>Same as <see cref="DIVIDE"/></summary>
    OEM_2 = 53,
    /// <summary>Same as <see cref="OEM_2"/></summary>
    DIVIDE = 53,

    RSHIFT = 54,
    MULTIPLY = 55,

    /// <summary>Same as <see cref="LMENU"/> and <see cref="RMENU"/></summary>
    MENU = 56,
    /// <summary>Same as <see cref="MENU"/></summary>
    LMENU = 56,
    /// <summary>Same as <see cref="MENU"/></summary>
    RMENU = 56,

    SPACE = 57,
    CAPITAL = 58,
    F1 = 59,
    F2 = 60,
    F3 = 61,
    F4 = 62,
    F5 = 0x3F,
    F6 = 0x40,
    F7 = 65,
    F8 = 66,
    F9 = 67,
    F10 = 68,
    NUMLOCK = 69,

    /// <summary>Same as <see cref="CANCEL"/></summary>
    SCROLL = 70,
    /// <summary>Same as <see cref="SCROLL"/></summary>
    CANCEL = 70,

    /// <summary>Same as <see cref="NUMPAD7"/></summary>
    HOME = 71,
    /// <summary>Same as <see cref="HOME"/></summary>
    NUMPAD7 = 71,

    /// <summary>Same as <see cref="NUMPAD8"/></summary>
    UP = 72,
    /// <summary>Same as <see cref="UP"/></summary>
    NUMPAD8 = 72,

    /// <summary>Same as <see cref="NUMPAD9"/></summary>
    PRIOR = 73,
    /// <summary>Same as <see cref="PRIOR"/></summary>
    NUMPAD9 = 73,
    
    SUBTRACT = 74,

    /// <summary>Same as <see cref="NUMPAD4"/></summary>
    LEFT = 75,
    /// <summary>Same as <see cref="LEFT"/></summary>
    NUMPAD4 = 75,

    /// <summary>Same as <see cref="NUMPAD5"/></summary>
    CLEAR = 76,
    /// <summary>Same as <see cref="CLEAR"/></summary>
    NUMPAD5 = 76,

    /// <summary>Same as <see cref="NUMPAD6"/></summary>
    RIGHT = 77,
    /// <summary>Same as <see cref="RIGHT"/></summary>
    NUMPAD6 = 77,

    ADD = 78,

    /// <summary>Same as <see cref="NUMPAD1"/></summary>
    END = 79,
    /// <summary>Same as <see cref="END"/></summary>
    NUMPAD1 = 79,

    /// <summary>Same as <see cref="NUMPAD2"/></summary>
    DOWN = 80,
    /// <summary>Same as <see cref="DOWN"/></summary>
    NUMPAD2 = 80,

    /// <summary>Same as <see cref="NUMPAD3"/></summary>
    NEXT = 81,
    /// <summary>Same as <see cref="NEXT"/></summary>
    NUMPAD3 = 81,

    /// <summary>Same as <see cref="NUMPAD0"/></summary>
    INSERT = 82,
    /// <summary>Same as <see cref="INSERT"/></summary>
    NUMPAD0 = 82,

    /// <summary>Same as <see cref="DECIMAL"/></summary>
    DELETE = 83,
    /// <summary>Same as <see cref="DELETE"/></summary>
    DECIMAL = 83,

    SNAPSHOT = 84,
    OEM_102 = 86,
    F11 = 87,
    F12 = 88,
    LWIN = 91,
    RWIN = 92,
    APPS = 93,
    EREOF = 93,
    SLEEP = 95,
    ZOOM = 98,
    HELP = 99,
    F13 = 100,

    /// <summary>Same as <see cref="BROWSER_SEARCH"/></summary>
    F14 = 101,
    /// <summary>Same as <see cref="F14"/></summary>
    BROWSER_SEARCH = 101,

    /// <summary>Same as <see cref="BROWSER_FAVORITES"/></summary>
    F15 = 102,
    /// <summary>Same as <see cref="F15"/></summary>
    BROWSER_FAVORITES = 102,

    /// <summary>Same as <see cref="BROWSER_REFRESH"/></summary>
    F16 = 103,
    /// <summary>Same as <see cref="F16"/></summary>
    BROWSER_REFRESH = 103,

    /// <summary>Same as <see cref="BROWSER_STOP"/></summary>
    F17 = 104,
    /// <summary>Same as <see cref="F17"/></summary>
    BROWSER_STOP = 104,

    /// <summary>Same as <see cref="BROWSER_FORWARD"/></summary>
    F18 = 105,
    /// <summary>Same as <see cref="F18"/></summary>
    BROWSER_FORWARD = 105,

    /// <summary>Same as <see cref="BROWSER_BACK"/></summary>
    F19 = 106,
    /// <summary>Same as <see cref="F19"/></summary>
    BROWSER_BACK = 106,

    /// <summary>Same as <see cref="LAUNCH_APP1"/></summary>
    F20 = 107,
    /// <summary>Same as <see cref="F20"/></summary>
    LAUNCH_APP1 = 107,

    /// <summary>Same as <see cref="LAUNCH_MAIL"/></summary>
    F21 = 108,
    /// <summary>Same as <see cref="F21"/></summary>
    LAUNCH_MAIL = 108,

    /// <summary>Same as <see cref="LAUNCH_MEDIA_SELECT"/></summary>
    F22 = 109,
    /// <summary>Same as <see cref="F22"/></summary>
    LAUNCH_MEDIA_SELECT = 109,

    F23 = 110,
    F24 = 118
}
