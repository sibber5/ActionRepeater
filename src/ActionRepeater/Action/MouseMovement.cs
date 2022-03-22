namespace ActionRepeater.Action;

public sealed class MouseMovement
{
    public Win32.POINT MovPoint { get; set; }

    public int DelayDuration { get; set; }

    public MouseMovement(Win32.POINT deltaMovPoint, int delayDuration)
    {
        MovPoint = deltaMovPoint;
        DelayDuration = delayDuration;
    }
}
