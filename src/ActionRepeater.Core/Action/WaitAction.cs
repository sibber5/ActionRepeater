using System.Text.Json.Serialization;
using System.Threading;
using ActionRepeater.Win32.Synch.Utilities;

namespace ActionRepeater.Core.Action;

public sealed class WaitAction : WaitableInputAction
{
    [JsonIgnore]
    public override string Name => "Wait";
    [JsonIgnore]
    public override string Description => ActionDescriptionTemplates.DurationMS(DurationMS);

    private int _durationMS;
    public int DurationMS
    {
        get => _durationMS;
        set
        {
            if (_durationMS == value) return;

            _durationMS = value;
            OnDescriptionChanged();
        }
    }

    public WaitAction(int durationMS)
    {
        _durationMS = durationMS;
    }

    /// <summary>Used only for deserialization.</summary>
    internal WaitAction() { }

    public override void PlayWait(HighResolutionWaiter waiter, CancellationToken cancellationToken)
    {
        using var registration = cancellationToken.Register(static (waiter) => ((HighResolutionWaiter)waiter!).Cancel(), waiter);
        waiter.Wait((uint)DurationMS);
    }
}
