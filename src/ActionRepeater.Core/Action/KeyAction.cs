using System;
using System.ComponentModel;
using ActionRepeater.Core.Input;
using ActionRepeater.Core.Extentions;
using VirtualKey = ActionRepeater.Win32.Input.VirtualKey;

namespace ActionRepeater.Core.Action;

public sealed class KeyAction : InputAction, IEquatable<KeyAction>
{
    public enum @Type
    {
        KeyPress = 1,
        KeyDown,
        KeyUp,
    }

    public @Type ActionType { get; }

    private string? _name;
    public override string Name
    {
        get
        {
            if (_name is null)
            {
                _name = ActionType.ToString().AddSpacesBetweenWords();
            }

            return _name;
        }
    }

    private string? _description;
    public override string Description
    {
        get
        {
            if (_description is null)
            {
                UpdateDescription();
            }

            return _description!;
        }
    }
    private void UpdateDescription()
    {
        _description = IsAutoRepeat
            ? ActionDescriptionTemplates.KeyAutoRepeat(_key)
            : ActionDescriptionTemplates.KeyFriendlyName(_key);
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
            RaisePropertyChanged(nameof(Description));
        }
    }

    public bool IsAutoRepeat { get; }

    public KeyAction(@Type type, VirtualKey key, bool isAutoRepeat = false)
    {
        ActionType = type;
        _key = key;
        IsAutoRepeat = isAutoRepeat;
    }

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
