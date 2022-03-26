namespace ActionRepeater.Action;

public sealed class MouseMovement
{
    public Win32.POINT MovPoint { get; set; }

    /// <summary>
    /// The actual measured delay duration.
    /// </summary>
    public int ActualDelay { get; set; }

    /// <summary>
    /// The delay duration with compensated for the system timer.
    /// </summary>
    public int DelayDuration { get => (int)(ActualDelay / 1.2); }

    public MouseMovement(Win32.POINT deltaMovPoint, int actualDelayDuration)
    {
        MovPoint = deltaMovPoint;
        ActualDelay = actualDelayDuration;
    }
}
