using System.ComponentModel;
using System.Text.Json.Serialization;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Input;
using ActionRepeater.Win32.Input;

namespace ActionRepeater.Core.Action;

public sealed class KeyAction : InputAction
{
    private KeyActionType _actionType;
    public KeyActionType ActionType
    {
        get => _actionType;
        set
        {
            if (_actionType == value) return;

            _actionType = value;
            OnNameChanged();
        }
    }

    [JsonIgnore]
    public override string Name => ActionType.ToString().AddSpacesBetweenWords();
    [JsonIgnore]
    public override string Description => IsAutoRepeat ? ActionDescriptionTemplates.KeyAutoRepeat(Key) : ActionDescriptionTemplates.KeyFriendlyName(Key);

    private VirtualKey _key = VirtualKey.NO_KEY;
    public VirtualKey Key
    {
        get => _key;
        set
        {
            if (_key == value) return;

            _key = value;
            OnDescriptionChanged();
        }
    }

    public bool IsAutoRepeat { get; }

    public KeyAction(KeyActionType type, VirtualKey key, bool isAutoRepeat = false)
    {
        ActionType = type;
        _key = key;
        IsAutoRepeat = isAutoRepeat;
    }

    public override void Play()
    {
        bool success = ActionType switch
        {
            KeyActionType.KeyDown => InputSimulator.SendKeyDown(_key),
            KeyActionType.KeyUp => InputSimulator.SendKeyUp(_key),
            KeyActionType.KeyPress => InputSimulator.SendKeyPress(_key),
            _ => throw new InvalidEnumArgumentException("Invalid key action."),
        };

        if (!success)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send key event ({ActionType}).");
            throw new Win32Exception(System.Runtime.InteropServices.Marshal.GetLastPInvokeError());
        }
    }
}

public enum KeyActionType
{
    KeyPress = 1,
    KeyDown,
    KeyUp,
}
