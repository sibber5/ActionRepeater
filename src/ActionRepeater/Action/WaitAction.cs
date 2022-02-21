using System;
using System.ComponentModel;

namespace ActionRepeater.Action;

internal sealed class WaitAction : IInputAction, IEquatable<WaitAction>
{
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public string Name => "Wait";

    private string _description;
    public string Description => _description;

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

    public void Play() => throw new NotImplementedException("Player should use it's own method to wait the specified amount.");

    public IInputAction Clone() => new WaitAction(_duration);

    public WaitAction(int duration)
    {
        _duration = duration;
        _description = ActionDescriptionTemplates.Duration(duration);
    }

    /// <summary>
    /// Checks if the object's values are equal.<br/>
    /// Use equality operators (== and !=) to check if the references are equal or not.
    /// </summary>
    public bool Equals(WaitAction other) => other is not null && other.Duration == _duration;

    /// <inheritdoc cref="Equals(WaitAction)"/>
    public override bool Equals(object obj) => Equals(obj as WaitAction);

    public override int GetHashCode() => _duration.GetHashCode();
}
