using System;
using Microsoft.UI.Xaml;
using ActionRepeater.Win32;
using ActionRepeater.Win32.WindowsAndMessages;
using ActionRepeater.UI.Utilities;
using ActionRepeater.UI.Services;

namespace ActionRepeater.UI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private const string MainWindowTitle = "ActionRepeater";
    private const int MainWindowWidth = 425;
    private const int MainWindowHeight = 600;

    public static MainWindow MainWindow { get; private set; } = null!;

    private readonly ActionHolder _copiedActionHolder = new();
    private readonly PathWindowService _pathWindowService = new();

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow { Title = MainWindowTitle };
        MainWindow.ViewModel = new(_copiedActionHolder, MainWindow.ShowContentDialog, _pathWindowService);

        // The Window object doesn't have Width and Height properties in WInUI 3.
        // You can use the Win32 API SetWindowPos to set the Width and Height.
        SetWindowSize(MainWindow.Handle, MainWindowWidth, MainWindowHeight);

        MainWindow.Activate();
    }

    private static void SetWindowSize(IntPtr hwnd, int width, int height)
    {
        // Win32 uses pixels and WinUI 3 uses effective pixels, so you should apply the DPI scale factor
        uint dpi = PInvoke.GetDpiForWindow(hwnd);
        float scalingFactor = dpi / 96f;
        width = (int)(width * scalingFactor);
        height = (int)(height * scalingFactor);

        PInvoke.SetWindowPos(hwnd, SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, height,
                                        SetWindowPosFlags.NOMOVE);
    }
}
