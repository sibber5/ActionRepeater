using System;

namespace ActionRepeater.Core.Action;

public sealed class WaitAction : InputAction, IEquatable<WaitAction>
{
    public override string Name => "Wait";

    private string? _description;
    public override string Description
    {
        get
        {
            if (_description is null)
            {
                _description = ActionDescriptionTemplates.Duration(Duration);
            }

            return _description!;
        }
    }

    private int _duration;
    public int Duration
    {
        get => _duration;
        set
        {
            if (_duration == value) return;

            _duration = value;
            _description = ActionDescriptionTemplates.Duration(value);
            RaisePropertyChanged(nameof(Description));
        }
    }

    public WaitAction(int duration)
    {
        _duration = duration;
    }

    /// <summary>Used only for deserialization.</summary>
    internal WaitAction() { }

    public override void Play() => throw new NotImplementedException("Player should use it's own method to wait the specified amount.");

    /// <summary>
    /// Checks if the object's values are equal.<br/>
    /// Use equality operators (== and !=) to check if the references are equal or not.
    /// </summary>
    public bool Equals(WaitAction? other) => other is not null && other.Duration == _duration;

    /// <inheritdoc cref="Equals(WaitAction)"/>
    public override bool Equals(object? obj) => Equals(obj as WaitAction);

    public override int GetHashCode() => _duration.GetHashCode();
}
