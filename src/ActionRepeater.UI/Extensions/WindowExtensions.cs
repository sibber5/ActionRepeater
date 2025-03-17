using ActionRepeater.Win32;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace ActionRepeater.UI.Extensions;

public static class WindowExtensions
{
    /// <summary>
    /// Win32 uses pixels and WinUI 3 uses effective pixels, so this method returns the dpi scale factor.
    /// </summary>
    public static float GetScalingFactor(this IWindowWithHandle window)
    {
        uint dpi = PInvoke.GetDpiForWindow(window.Handle);
        return dpi / 96f;
    }

    /// <summary>
    /// Sets window size, accounting for dpi.
    /// </summary>
    public static void SetSize(this IWindowWithHandle window, SizeInt32 size)
    {
        float scalingFactor = window.GetScalingFactor();
        size.Width = (int)(size.Width * scalingFactor);
        size.Height = (int)(size.Height * scalingFactor);

        ((Window)window).AppWindow.Resize(size);
    }

    /// <summary>
    /// Sets window position, accounting for dpi.
    /// </summary>
    public static void SetPosition(this IWindowWithHandle window, PointInt32 position)
    {
        float scalingFactor = window.GetScalingFactor();
        position.X = (int)(position.X * scalingFactor);
        position.Y = (int)(position.Y * scalingFactor);
         
        ((Window)window).AppWindow.Move(position);
    }
}
