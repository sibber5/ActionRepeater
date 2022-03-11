using System;

namespace ActionRepeater.Action;

public sealed class WaitAction : InputAction, IEquatable<WaitAction>
{
    public override string Name { get => "Wait"; }

    private string _description;
    public override string Description { get => _description; }

    private int _duration;
    public int Duration
    {
        get => _duration;
        set
        {
            if (_duration == value) return;

            _duration = value;
            _description = ActionDescriptionTemplates.Duration(value);
            NotifyPropertyChanged(nameof(Description));
        }
    }

    public override void Play() => throw new NotImplementedException("Player should use it's own method to wait the specified amount.");

    public override InputAction Clone() => new WaitAction(_duration);

    public WaitAction(int duration)
    {
        _duration = duration;
        _description = ActionDescriptionTemplates.Duration(duration);
    }

    /// <summary>
    /// Checks if the object's values are equal.<br/>
    /// Use equality operators (== and !=) to check if the references are equal or not.
    /// </summary>
    public bool Equals(WaitAction? other) => other is not null && other.Duration == _duration;

    /// <inheritdoc cref="Equals(WaitAction)"/>
    public override bool Equals(object? obj) => Equals(obj as WaitAction);

    public override int GetHashCode() => _duration.GetHashCode();

    public static async System.Threading.Tasks.Task<WaitAction> CreateActionFromXmlAsync(System.Xml.XmlReader reader)
    {
        await reader.ReadAsync();
        if (!reader.Name.Equals(nameof(Duration), StringComparison.Ordinal))
        {
            throw new FormatException($"Unexpected element \"{reader.Name}\". Expected \"{nameof(Duration)}\".");
        }
        return new WaitAction(reader.ReadElementContentAsInt());
    }

    public override void WriteXml(System.Xml.XmlWriter writer)
    {
        writer.WriteAttributeString("Type", nameof(WaitAction));

        writer.WriteComment("Time in milliseconds");
        writer.WriteElementString(nameof(Duration), _duration.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}
