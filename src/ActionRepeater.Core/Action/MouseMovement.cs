using System.Linq;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Win32;

namespace ActionRepeater.Core.Action;

public sealed class MouseMovement
{
    /// <summary>
    /// The movement delta, in both <i>x</i> and <i>y</i> directions.
    /// </summary>
    public POINT Delta { get; set; }

    public long DelayDurationNS { get; set; }

    public MouseMovement(POINT delta, long delayNanoseconds)
    {
        Delta = delta;
        DelayDurationNS = delayNanoseconds;
    }

    public static POINT OffsetPointWithinScreens(POINT point, POINT offset)
    {
        var monitors = SystemInformation.MonitorRects;

        var ogMonitor = monitors.First(m => m.ContainsInclusive(point.x, point.y));

        point.x += offset.x;
        if (!monitors.Any(m => m.ContainsInclusive(point.x, point.y)))
        {
            point.x = offset.x < 0 ? ogMonitor.Left : ogMonitor.Right;
        }

        point.y += offset.y;
        if (!monitors.Any(m => m.ContainsInclusive(point.x, point.y)))
        {
            point.y = offset.y < 0 ? ogMonitor.Top : ogMonitor.Bottom;
        }

        return point;
    }
}
