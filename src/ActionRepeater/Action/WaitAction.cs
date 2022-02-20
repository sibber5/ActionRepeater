using System;
using System.ComponentModel;

namespace ActionRepeater.Action;

internal sealed class WaitAction : IInputAction
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
}
