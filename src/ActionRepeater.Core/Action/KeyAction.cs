using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Input;
using VirtualKey = ActionRepeater.Win32.Input.VirtualKey;

namespace ActionRepeater.Core.Action;

public sealed class KeyAction : InputAction, IEquatable<KeyAction>
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

    /// <summary>
    /// Checks if the object's values are equal.<br/>
    /// Use equality operators (== and !=) to check if the references are equal or not.
    /// </summary>
    public bool Equals(KeyAction? other) => other is not null
        && other.ActionType == ActionType
        && other.Key == _key
        && other.IsAutoRepeat == IsAutoRepeat;

    /// <inheritdoc cref="Equals(KeyAction)"/>
    public override bool Equals(object? obj) => Equals(obj as KeyAction);

    public override int GetHashCode() => HashCode.Combine(ActionType, _key, IsAutoRepeat);
}

public enum KeyActionType
{
    KeyPress = 1,
    KeyDown,
    KeyUp,
}
