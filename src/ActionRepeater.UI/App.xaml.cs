using System;
using System.Linq;
using Microsoft.UI.Xaml;
using ActionRepeater.UI.Services;
using ActionRepeater.Win32;
using ActionRepeater.Win32.WindowsAndMessages;

namespace ActionRepeater.UI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private const string MainWindowTitle = "ActionRepeater";
    private const int MainWindowWidth = 505;
    private const int MainWindowHeight = 600;

    public static MainWindow MainWindow { get; private set; } = null!;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();

        string[] args = Environment.GetCommandLineArgs();

        const string themeParam = "--theme=";
        string? themeArg = args.FirstOrDefault(x => x.StartsWith(themeParam));
        if (themeArg is not null)
        {
            App.Current.RequestedTheme = themeArg[themeParam.Length..] switch
            {
                "dark" => ApplicationTheme.Dark,
                "light" => ApplicationTheme.Light,
                _ => throw new NotSupportedException()
            };
        }
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new() { Title = MainWindowTitle };

        ContentDialogService cds = new();

        MainWindow.ViewModel = new(cds, new PathWindowService());

        // XamlRoot has to be set after Content has loaded
        ((FrameworkElement)MainWindow.Content).Loaded += (_, _) =>
        {
            cds.XamlRoot = App.MainWindow.GridXamlRoot;
            System.Diagnostics.Debug.WriteLine("XamlRoot set.");
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
