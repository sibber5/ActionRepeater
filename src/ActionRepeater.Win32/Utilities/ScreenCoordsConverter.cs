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

    public static POINT GetAbsCoordFromVirtScreenPoint(POINT virtScreenPoint)
    {
        int x = (int)MathF.Round(((float)virtScreenPoint.x / VirtualScreen.Width) * ushort.MaxValue);
        int y = (int)MathF.Round(((float)virtScreenPoint.y / VirtualScreen.Height) * ushort.MaxValue);
        return new(x, y);
    }

    public static POINT GetAbsCoordFromPosRelToPrimary(POINT posRelToPrimaryOrigin)
    {
        POINT virtScreenPoint = GetVirtScreenPosFromPosRelToPrimary(posRelToPrimaryOrigin);
        return GetAbsCoordFromVirtScreenPoint(virtScreenPoint);
    }

    public static POINT GetPosRelToPrimaryFromVirtScreenPoint(POINT virtScreenPoint)
    {
        int x = virtScreenPoint.x + PrimaryMonitorRect.Width - VirtualScreen.Width;
        int y = virtScreenPoint.y + PrimaryMonitorRect.Height - VirtualScreen.Height;
        return new(x, y);
    }
}
