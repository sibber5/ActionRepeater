﻿using System;
using Microsoft.UI.Xaml;
using ActionRepeater.UI.Services;
using ActionRepeater.Win32;
using ActionRepeater.Win32.WindowsAndMessages;
using System.Threading.Tasks;
using System.IO;
using ActionRepeater.Core.Helpers;
using ActionRepeater.UI.Utilities;

namespace ActionRepeater.UI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private const string MainWindowTitle = "ActionRepeater";
    private const int MainWindowWidth = 507;
    private const int MainWindowHeight = 600;

    public static string AppDataOptionsDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(ActionRepeater));
    public static string AppFolderOptionsDir => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!;
    public static string OptionsFileName => "Options.json";

    public static MainWindow MainWindow { get; private set; } = null!;

    private static readonly PathWindowService _pathWindowService = new();
    private static readonly ContentDialogService _contentDialogService = new();

    private static bool _saveOptions;
    private static bool _saveOnExit = true;

    private static Exception? _loadingOptionsException;

    private static ObservablePropertyReverter<OptionsFileLocation>? _optsFileLocReverter;

    // used to check what UIOptions.Theme was changed from, when its changed
    private static bool _wasAppThemeSet = false;

    private static bool _isRevertingTheme = false;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();

        if (!TryLoadOptions(Path.Combine(AppDataOptionsDir, OptionsFileName)))
        {
            TryLoadOptions(Path.Combine(AppFolderOptionsDir, OptionsFileName));
        }

        _saveOptions = UIOptions.Instance.OptionsFileLocation != OptionsFileLocation.None;

        switch (UIOptions.Instance.Theme)
        {
            case Theme.Light:
                App.Current.RequestedTheme = ApplicationTheme.Light;
                _wasAppThemeSet = true;
                break;

            case Theme.Dark:
                App.Current.RequestedTheme = ApplicationTheme.Dark;
                _wasAppThemeSet = true;
                break;
        }

        UIOptions.Instance.PropertyChanging += UIOptions_PropertyChanging;
        UIOptions.Instance.PropertyChanged += UIOptions_PropertyChanged;
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Behaviors.AddActionMenuBehavior.ContentDialogService = _contentDialogService;

        MainWindow = new()
        {
            Title = MainWindowTitle,
            ViewModel = new(_contentDialogService, _pathWindowService)
        };

        // XamlRoot has to be set after Content has loaded
        ((FrameworkElement)MainWindow.Content).Loaded += static async (_, _) =>
        {
            _contentDialogService.XamlRoot = App.MainWindow.GridXamlRoot;
            System.Diagnostics.Debug.WriteLine("XamlRoot set.");

            if (_loadingOptionsException is not null)
            {
                await _contentDialogService.ShowErrorDialog("Could not load options", _loadingOptionsException.Message);
            }
        };

        MainWindow.Closed += MainWindow_Closed;

        // The Window object doesn't have Width and Height properties in WInUI 3.
        // You can use the Win32 API SetWindowPos to set the Width and Height.
        SetWindowSize(MainWindow.Handle, MainWindowWidth, MainWindowHeight);

        MainWindow.Activate();
    }

    private static async void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        if (!_saveOnExit) return;

        await SaveOptions();
    }

    private static async void UIOptions_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
    {
        if (e.PropertyName == nameof(UIOptions.Instance.OptionsFileLocation))
        {
            if (_optsFileLocReverter?.IsReverting == true) return;

            if (_optsFileLocReverter is null)
            {
                _optsFileLocReverter = new(UIOptions.Instance.OptionsFileLocation,
                    static () => UIOptions.Instance.OptionsFileLocation,
                    static (val) => UIOptions.Instance.OptionsFileLocation = val);
            }
            else
            {
                _optsFileLocReverter.PreviousValue = UIOptions.Instance.OptionsFileLocation;
            }

            switch (UIOptions.Instance.OptionsFileLocation)
            {
                case OptionsFileLocation.AppData:
                    if (!Directory.Exists(AppDataOptionsDir)) break;

                    await _contentDialogService.ShowYesNoMessageDialog(
                        "Options file in AppData will be deleted",
                        "Are you sure you want to change the options file location?",
                        onYesClick: static () => Directory.Delete(AppDataOptionsDir, true),
                        onNoClick: _optsFileLocReverter.Revert);
                    break;

                case OptionsFileLocation.AppFolder:
                    string path = Path.Combine(AppFolderOptionsDir, OptionsFileName);

                    if (!File.Exists(path)) break;

                    await _contentDialogService.ShowYesNoMessageDialog(
                        "Options file in app folder will be deleted",
                        "Are you sure you want to change the options file location?",
                        onYesClick: static () => File.Delete(Path.Combine(AppFolderOptionsDir, OptionsFileName)),
                        onNoClick: _optsFileLocReverter.Revert);
                    break;
            }
        }
    }

    private static async void UIOptions_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(UIOptions.Instance.Theme):
                if (_isRevertingTheme) break;

                Action revertThemeOption = static () =>
                {
                    _isRevertingTheme = true;
                    MainWindow.DispatcherQueue.TryEnqueue(static () =>
                    {
                        Theme previousTheme;
                        if (_wasAppThemeSet)
                        {
                            previousTheme = UIOptions.Instance.Theme == Theme.Light ? Theme.Dark : Theme.Light;
                        }
                        else
                        {
                            previousTheme = Theme.WindowsSetting;
                        }

                        UIOptions.Instance.Theme = previousTheme;
                        _isRevertingTheme = false;
                    });
                };

                if (UIOptions.Instance.OptionsFileLocation == OptionsFileLocation.None)
                {
                    revertThemeOption();
                    await _contentDialogService.ShowMessageDialog(
                        "Saving is disabled",
                        "Options saving must be enabled in order to change the theme. (the theme must be applied on startup)");
                    break;
                }

                await _contentDialogService.ShowYesNoMessageDialog("Restart required to change theme", "Restart?",
                    onYesClick: static async () =>
                    {
                        string path = Path.ChangeExtension(System.Reflection.Assembly.GetEntryAssembly()!.Location, ".exe");

                        await SaveOptions();
                        _saveOnExit = false;

                        System.Diagnostics.Process.Start(path);
                        Application.Current.Exit();

                        //Microsoft.Windows.AppLifecycle.AppInstance.Restart($"--theme={(isDarkMode ? "light" : "dark")}"); // throws FileNotFoundException
                    },
                    onNoClick: revertThemeOption);
                break;

            case nameof(UIOptions.Instance.OptionsFileLocation):
                _saveOptions = UIOptions.Instance.OptionsFileLocation != OptionsFileLocation.None;
                break;
        }
    }

    private static async Task SaveOptions()
    {
        if (!_saveOptions) return;

        try
        {
            string path;

            switch (UIOptions.Instance.OptionsFileLocation)
            {
                case OptionsFileLocation.AppData:
                    path = Path.Combine(AppDataOptionsDir, OptionsFileName);
                    Directory.CreateDirectory(AppDataOptionsDir);
                    break;

                case OptionsFileLocation.AppFolder:
                    path = Path.Combine(AppFolderOptionsDir, OptionsFileName);
                    break;

                case OptionsFileLocation.None:
                    return;

                default:
                    throw new InvalidOperationException($"{nameof(UIOptions.Instance.OptionsFileLocation)} contains an invalid value.");
            }

            AllOptions options = new(Core.Options.Instance, UIOptions.Instance);
            await SerializationHelper.SerializeAsync(options, path);
        }
        catch (Exception ex)
        {
            await _contentDialogService.ShowErrorDialog("Could not save options.", ex.Message);
        }
    }

    private static bool TryLoadOptions(string path)
    {
        try
        {
            if (!File.Exists(path)) return false;

            AllOptions options = System.Text.Json.JsonSerializer.Deserialize<AllOptions>(File.ReadAllText(path));
            Core.Options.Load(options.CoreOptions);
            UIOptions.Load(options.UIOptions);

            return true;
        }
        catch (Exception ex)
        {
            _loadingOptionsException = ex;
            return false;
        }
    }

    private static void SetWindowSize(IntPtr hwnd, int width, int height)
    {
        // Win32 uses pixels and WinUI 3 uses effective pixels, so you should apply the DPI scale factor
        uint dpi = PInvoke.GetDpiForWindow(hwnd);
        float scalingFactor = dpi / 96f;
        width = (int)(width * scalingFactor);
        height = (int)(height * scalingFactor);

        PInvoke.SetWindowPos(hwnd, SpecialWindowHandles.HWND_TOP, 0, 0, width, height, SetWindowPosFlags.NOMOVE);
    }
}

public record struct AllOptions(Core.Options CoreOptions, UIOptions UIOptions);
