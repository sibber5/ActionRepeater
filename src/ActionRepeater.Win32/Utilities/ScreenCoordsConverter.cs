using System;
using static ActionRepeater.Win32.SystemInformation;

namespace ActionRepeater.Win32.Utilities;

public static class ScreenCoordsConverter
{
    public static POINT GetVirtScreenPosFromPosRelToPrimary(POINT posRelToPrimaryOrigin)
    {
        int x = posRelToPrimaryOrigin.x - PrimaryMonitorRect.Width + VirtualScreen.Width;
        int y = posRelToPrimaryOrigin.y - PrimaryMonitorRect.Height + VirtualScreen.Height;
        return new(x, y);
    }

    public static POINT GetAbsoluteCoordinateFromVirtScreenPoint(POINT virtScreenPoint)
    {
        int x = (int)MathF.Round(((float)virtScreenPoint.x / VirtualScreen.Width) * 65535);
        int y = (int)MathF.Round(((float)virtScreenPoint.y / VirtualScreen.Height) * 65535);
        return new(x, y);
    }

    public static POINT GetAbsoluteCoordinateFromPosRelToPrimary(POINT posRelToPrimaryOrigin)
    {
        POINT virtScreenPoint = GetVirtScreenPosFromPosRelToPrimary(posRelToPrimaryOrigin);
        return GetAbsoluteCoordinateFromVirtScreenPoint(virtScreenPoint);
    }
}
