using System;
using ActionRepeater.Core.Action;

namespace ActionRepeater.UI.Helpers;

public static class ActionGlyphs
{
    public const string Keyboard = "\uE92E";
    public const string Mouse = "\uE962";
    public const string Clock = "\uED5A";

    public static (string? Glyph, double Size) GetIconForAction(InputAction action) => action switch
    {
        KeyAction => (Keyboard, 22),
        MouseButtonAction => (Mouse, 20),
        MouseWheelAction => (Mouse, 20),
        WaitAction => (Clock, 18),
        TextTypeAction => (Keyboard, 22),
        _ => throw new NotImplementedException()
    };
}
