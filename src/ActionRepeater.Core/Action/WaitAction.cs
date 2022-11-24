using System.Text.Json.Serialization;
using ActionRepeater.Win32.Synch.Utilities;

namespace ActionRepeater.Core.Action;

public sealed class WaitAction : WaitableInputAction
{
    [JsonIgnore]
    public override string Name => "Wait";
    [JsonIgnore]
    public override string Description => ActionDescriptionTemplates.Duration(Duration);

    private int _duration;
    public int Duration
    {
        get => _duration;
        set
        {
            if (_duration == value) return;

            _duration = value;
            OnDescriptionChanged();
        }
    }

    public WaitAction(int duration)
    {
        _duration = duration;
    }

    /// <summary>Used only for deserialization.</summary>
    internal WaitAction() { }

    public override void PlayWait(HighResolutionWaiter waiter)
    {
        waiter.Wait((uint)Duration);
    }
}
