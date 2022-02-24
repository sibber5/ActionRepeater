using System.ComponentModel;
using ActionRepeater.Input;
using VirtualKey = ActionRepeater.Win32.Input.VirtualKey;

namespace ActionRepeater.Action;

public sealed class KeyAction : InputAction, System.IEquatable<KeyAction>
{
    public enum @Type
    {
        KeyDown,
        KeyUp,
        KeyPress
    }

    public @Type ActionType { get; }

    public override string Name
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
    public override string Description => _description;

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

    public override InputAction Clone() => new KeyAction(ActionType, _description, _key, IsAutoRepeat);

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

    public static async System.Threading.Tasks.Task<KeyAction> CreateActionFromXmlAsync(System.Xml.XmlReader reader)
    {
        await reader.ReadAsync(); // move to Start Element ActionType

        ThrowIfInvalidName(nameof(ActionType));
        @Type type = (@Type)reader.ReadElementContentAsInt(); // this moves to start of next element

        ThrowIfInvalidName(nameof(Key));
        VirtualKey key = (VirtualKey)reader.ReadElementContentAsInt();

        ThrowIfInvalidName(nameof(IsAutoRepeat));
        bool isAutoRepeat = reader.ReadElementContentAsBoolean();

        return new KeyAction(type, key, isAutoRepeat);

        void ThrowIfInvalidName(string name)
        {
            if (!reader.Name.Equals(name, System.StringComparison.Ordinal))
            {
                throw new System.FormatException($"Unexpected element \"{reader.Name}\". Expected \"{name}\".");
            }
        }
    }

    public override void WriteXml(System.Xml.XmlWriter writer)
    {
        writer.WriteAttributeString("Type", nameof(KeyAction));

        writer.WriteComment("ActionType is the type of the key action. it can be on of the following: 0 - KeyDown; 1 - KeyUp; 2 - KeyPress");
        writer.WriteElementString(nameof(ActionType), ((int)ActionType).ToString(System.Globalization.CultureInfo.InvariantCulture));
        writer.WriteElementString(nameof(Key), ((ushort)_key).ToString(System.Globalization.CultureInfo.InvariantCulture));
        writer.WriteElementString(nameof(IsAutoRepeat), IsAutoRepeat.ToString(System.Globalization.CultureInfo.InvariantCulture).ToLowerInvariant());
    }
}
