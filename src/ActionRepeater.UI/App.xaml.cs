using System;
using Microsoft.UI.Xaml;
using ActionRepeater.Win32;
using ActionRepeater.Win32.WindowsAndMessages;
using ActionRepeater.UI.Utilities;

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

    //public static bool IsPathWindowOpen => _pathWindow is not null;

    //private static PathWindow? _pathWindow;
    //private static int _lastCursorPtsCount;
    //private static Win32.POINT? _lastAbsPt;

    private readonly ActionHolder _copiedActionHolder = new();

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Opens the path window, unless the cursor path is empty.
    /// </summary>
    /// <returns>true if the function succeeds (the window has been opened), otherwise false (fails if the cursor path is empty).</returns>
    public static void OpenPathWindow()
    {
        //Debug.Assert(_pathWindow is null, "Path window is not null.");

        //if (ActionManager.CursorPathStart is null)
        //{
        //    _pathWindow = new();
        //    _lastAbsPt = null;
        //}
        //else
        //{
        //    var absCursorPts = ActionManager.AbsoluteCursorPath.Select(p => (System.Drawing.Point)p.MovPoint).ToArray();
        //    _lastAbsPt = absCursorPts[^1];

        //    _pathWindow = new(absCursorPts);
        //}

        //UpdatePathWindow();

        throw new NotImplementedException();
    }

    public static void ClosePathWindow()
    {
        //Debug.Assert(_pathWindow is not null, "Path window is null.");

        //_pathWindow.Dispose();
        //_pathWindow = null;

        throw new NotImplementedException();
    }

    private static void UpdatePathWindow()
    {
        //System.Threading.Tasks.Task.Run(async () =>
        //{
        //    Debug.WriteLine("Update Window Task Started.");

        //    var cursorPath = ActionManager.CursorPath;

        //    _lastCursorPtsCount = cursorPath.Count;

        //    while (_pathWindow is not null)
        //    {
        //        await System.Threading.Tasks.Task.Delay(40);

        //        if (_lastCursorPtsCount == cursorPath.Count) continue;

        //        if (_pathWindow is null) break;

        //        if (cursorPath.Count == 0)
        //        {
        //            _lastAbsPt = ActionManager.CursorPathStart?.MovPoint;
        //            _lastCursorPtsCount = cursorPath.Count;
        //            _pathWindow.ClearPath();
        //            continue;
        //        }

        //        _lastAbsPt ??= ActionManager.CursorPathStart!.MovPoint;

        //        for (int i = _lastCursorPtsCount; i < cursorPath.Count; ++i)
        //        {
        //            var newPoint = Core.Action.MouseMovement.OffsetPointWithinScreens(_lastAbsPt.Value, cursorPath[i].MovPoint);
        //            _pathWindow.AddLineToPath(_lastAbsPt.Value, newPoint);
        //            _lastAbsPt = newPoint;
        //        }

        //        _lastCursorPtsCount = cursorPath.Count;
        //    }

        //    Debug.WriteLine("Update Window Task Finished.");
        //});

        throw new NotImplementedException();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow
        {
            Title = MainWindowTitle,
            ViewModel = new(_copiedActionHolder)
        };

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
