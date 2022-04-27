using System.Linq;
using ActionRepeater.Extentions;
using ActionRepeater.Win32;

namespace ActionRepeater.Action;

public sealed class MouseMovement
{
    public POINT MovPoint { get; set; }

    /// <summary>
    /// The actual measured delay duration.
    /// </summary>
    public int ActualDelay { get; set; }

    /// <summary>
    /// The delay duration with compensated for the system timer.
    /// </summary>
    public int DelayDuration { get => (int)(ActualDelay / 1.2); }

    // for serialization
    private MouseMovement() { }

    public MouseMovement(POINT deltaMovPoint, int actualDelayDuration)
    {
        MovPoint = deltaMovPoint;
        ActualDelay = actualDelayDuration;
    }

    public static POINT OffsetPointWithinScreens(POINT point, POINT offset)
    {
        var monitors = SystemInformation.MonitorRects;

        var ogMonitor = monitors.First(m => m.ContainsInclusive(point.x, point.y));

        point.x += offset.x;
        if (!monitors.Any(m => m.ContainsInclusive(point.x, point.y)))
        {
            if (offset.x < 0)
            {
                point.x = ogMonitor.Left;
            }
            else
            {
                point.x = ogMonitor.Right;
            }
        }

        point.y += offset.y;
        if (!monitors.Any(m => m.ContainsInclusive(point.x, point.y)))
        {
            if (offset.y < 0)
            {
                point.y = ogMonitor.Top;
            }
            else
            {
                point.y = ogMonitor.Bottom;
            }
        }

        return point;
    }
}
