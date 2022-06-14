using System.ComponentModel;
using ActionRepeater.Core.Input;
using ActionRepeater.Core.Extentions;
using VirtualKey = ActionRepeater.Win32.Input.VirtualKey;

namespace ActionRepeater.Core.Action;

// This is intentional because when selecting an action from the ui, two identical ones would not be considered the same one
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

public sealed class KeyAction : InputAction, System.IEquatable<KeyAction>
{
    public enum @Type
    {
        KeyDown,
        KeyUp,
        KeyPress
    }

    public @Type ActionType { get; }

    public override string Name { get; }

    private string _description = default!;
    public override string Description { get => _description; }
    private void UpdateDescription()
    {
        if (IsAutoRepeat)
        {
            _description = ActionDescriptionTemplates.KeyAutoRepeat(_key);
            return;
        }

        _description = ActionDescriptionTemplates.KeyFriendlyName(_key);
    }

    private VirtualKey _key = VirtualKey.NO_KEY;
    public VirtualKey Key
    {
        get => _key;
        set
        {
            if (_key == value) return;

            _key = value;
            UpdateDescription();
            NotifyPropertyChanged(nameof(Description));
        }
    }

    public bool IsAutoRepeat { get; }

    public override void Play()
    {
        bool success = ActionType switch
        {
            @Type.KeyDown => InputSimulator.SendKeyDown(_key),
            @Type.KeyUp => InputSimulator.SendKeyUp(_key),
            @Type.KeyPress => InputSimulator.SendKeyPress(_key),
            _ => throw new InvalidEnumArgumentException("Invalid key action."),
        };

        if (!success)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send key event ({ActionType}).");
            throw new Win32Exception(System.Runtime.InteropServices.Marshal.GetLastPInvokeError());
        }
    }

    //public override InputAction Clone() => new KeyAction(ActionType, _key, IsAutoRepeat);

    public KeyAction(@Type type, VirtualKey key, bool isAutoRepeat = false)
    {
        ActionType = type;
        _key = key;
        IsAutoRepeat = isAutoRepeat;
        Name = type.ToString().AddSpacesBetweenWords();
        UpdateDescription();
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

    //public override int GetHashCode() => System.HashCode.Combine(ActionType, _key, IsAutoRepeat);

    private KeyAction() { Name = ActionType.ToString().AddSpacesBetweenWords(); }
}
