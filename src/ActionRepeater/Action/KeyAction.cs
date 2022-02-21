using System.ComponentModel;
using ActionRepeater.Input;
using VirtualKey = ActionRepeater.Win32.Input.VirtualKey;

namespace ActionRepeater.Action;

internal sealed class KeyAction : IInputAction, System.IEquatable<KeyAction>
{
    public enum @Type
    {
        KeyDown,
        KeyUp,
        KeyPress
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public @Type ActionType { get; }

    public string Name
    {
        get
        {
            System.Text.StringBuilder sb = new();
            string name = ActionType.ToString();
            for (int i = 1; i < name.Length; ++i)
            {
                sb.Append(name[i - 1]);
                if (char.IsUpper(name[i]))
                {
                    sb.Append(' ');
                }
            }
            sb.Append(name[^1]);
            return sb.ToString();
        }
    }

    private string _description;
    public string Description => _description;

    private VirtualKey _key = VirtualKey.NO_KEY;
    public VirtualKey Key
    {
        get => _key;
        set
        {
            if (_key == value) return;

            _key = value;
            if (IsAutoRepeat)
            {
                _description = ActionDescriptionTemplates.KeyAutoRepeat(value);
            }
            else
            {
                _description = ActionDescriptionTemplates.KeyFriendlyName(value);
            }
            NotifyPropertyChanged(nameof(Description));
        }
    }

    public bool IsAutoRepeat { get; }

    public void Play()
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

    public IInputAction Clone() => new KeyAction(ActionType, _description, _key, IsAutoRepeat);

    private KeyAction(@Type type, string description, VirtualKey key, bool isAutoRepeat)
    {
        ActionType = type;
        _description = description;
        _key = key;
        IsAutoRepeat = isAutoRepeat;
    }

    public KeyAction(@Type type, VirtualKey key, bool isAutoRepeat = false)
    {
        ActionType = type;
        _key = key;
        IsAutoRepeat = isAutoRepeat;
        if (isAutoRepeat)
        {
            _description = ActionDescriptionTemplates.KeyAutoRepeat(key);
        }
        else
        {
            _description = ActionDescriptionTemplates.KeyFriendlyName(key);
        }
    }

    /// <summary>
    /// Checks if the object's values are equal.<br/>
    /// Use equality operators (== and !=) to check if the references are equal or not.
    /// </summary>
    public bool Equals(KeyAction other) => other is not null
        && other.ActionType == ActionType
        && other.Key == _key
        && other.IsAutoRepeat == IsAutoRepeat;

    /// <inheritdoc cref="Equals(KeyAction)"/>
    public override bool Equals(object obj) => Equals(obj as KeyAction);

    public override int GetHashCode() => System.HashCode.Combine(ActionType, _key, IsAutoRepeat);
}
