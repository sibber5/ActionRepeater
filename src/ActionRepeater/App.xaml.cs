using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.UI.Xaml;
using PathWindows;

[assembly: SupportedOSPlatform("Windows10.0.17763.0")]
namespace ActionRepeater;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private const string MainWindowTitle = "ActionRepeater";
    private const int MainWindowWidth = 425;
    private const int MainWindowHeight = 600;
    //private const int MainWindowMinWidth = 315;
    //private const int MainWindowMinHeight = 100;

    public static MainWindow MainWindow { get; private set; } = null!;

    public static bool IsPathWindowOpen => _pathWindow is not null;

    private static PathWindow? _pathWindow;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Opens the path window, unless the cursor path is empty.
    /// </summary>
    /// <returns>true if the function succeeds (the window has been opened), otherwise false (fails if the cursor path is empty).</returns>
    public static bool TryOpenPathWindow()
    {
        Debug.Assert(_pathWindow is null, "Path window is not null.");

        //if (Input.ActionManager.CursorPathStart is null)
        //{
        //    return false;
        //}

        _pathWindow = new(Input.ActionManager.CursorPathStart is null ? null
            : Input.ActionManager.AbsoluteCursorPath.Select(p => (System.Drawing.Point)p.MovPoint).ToArray());

        //Input.ActionManager.CursorPath.CollectionChanged += UpdatePathWindow;
        UpdatePathWindow();

        return true;
    }

    public static void ClosePathWindow()
    {
        Debug.Assert(_pathWindow is not null, "Path window is null.");

        //Input.ActionManager.CursorPath.CollectionChanged -= UpdatePathWindow;

        _pathWindow.Dispose();
        _pathWindow = null;
        //GC.Collect();
    }

    private static void UpdatePathWindow()
    {
        System.Threading.Tasks.Task.Run(async () =>
        {
            while (true)
            {
                await System.Threading.Tasks.Task.Delay(40);

                if (Input.ActionManager.CursorPathStart is null || !Input.Recorder.IsRecording) continue;

                if (_pathWindow is null) break;
                _pathWindow.OnPathChanged(Input.ActionManager.AbsoluteCursorPath.Select(p => (System.Drawing.Point)p.MovPoint).ToArray());
            }
        });
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow() { Title = MainWindowTitle };

        // The Window object doesn't have Width and Height properties in WInUI 3.
        // You can use the Win32 API SetWindowPos to set the Width and Height.
        SetWindowSize(MainWindow.Handle, MainWindowWidth, MainWindowHeight);

        MainWindow.Activate();
    }

    private static void SetWindowSize(IntPtr hwnd, int width, int height)
    {
        // Win32 uses pixels and WinUI 3 uses effective pixels, so you should apply the DPI scale factor
        uint dpi = Win32.PInvoke.GetDpiForWindow(hwnd);
        float scalingFactor = dpi / 96f;
        width = (int)(width * scalingFactor);
        height = (int)(height * scalingFactor);

        Win32.PInvoke.SetWindowPos(hwnd, Win32.WindowsAndMessages.SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, height,
                                        Win32.WindowsAndMessages.SetWindowPosFlags.NOMOVE);
    }
}
