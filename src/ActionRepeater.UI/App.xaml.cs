using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ActionRepeater.Core;
using ActionRepeater.Core.Helpers;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Factories;
using ActionRepeater.UI.Services;
using ActionRepeater.UI.Utilities;
using ActionRepeater.UI.ViewModels;
using ActionRepeater.UI.Views;
using ActionRepeater.UI.Views.HomeViewRibbons;
using ActionRepeater.Win32;
using ActionRepeater.Win32.Synch.Utilities;
using ActionRepeater.Win32.WindowsAndMessages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace ActionRepeater.UI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public static string AppDataOptionsDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(ActionRepeater));
    public static string OptionsFileName => "Options.json";

    public IServiceProvider Services { get; }

    private static new App Current => (App)Application.Current;

    private MainWindow _mainWindow = null!;

    private readonly AppOptions _options;

    private readonly IDialogService _dialogService;
    private readonly IDispatcher _dispatcher;

    private bool _saveOnExit = true;

    private Exception? _loadingOptionsException;

    private ObservablePropertyReverter<OptionsFileLocation>? _optsFileLocReverter;

    private ObservablePropertyReverter<Theme>? _themeOptionReverter;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        _options = null!;
        if (!TryLoadOptions(Path.Combine(AppDataOptionsDir, OptionsFileName), out _options!))
        {
            TryLoadOptions(Path.Combine(AppContext.BaseDirectory, OptionsFileName), out _options!);
        }

        _options ??= new(new CoreOptions(), new UIOptions());

        Services = ConfigureServices();

        _dialogService = Services.GetRequiredService<IDialogService>();
        _dispatcher = Services.GetRequiredService<IDispatcher>();

        InitializeComponent();

        string[] args = Environment.GetCommandLineArgs();
        string? themeArg = args.FirstOrDefault(static x => x.StartsWith("--theme="));
        if (themeArg is not null)
        {
            var theme = themeArg.AsSpan("--theme=".Length);
            _options.UI.Theme = theme switch
            {
                "light" => Theme.Light,
                "dark" => Theme.Dark,
                _ => throw new NotSupportedException(),
            };
        }

        switch (_options.UI.Theme)
        {
            case Theme.Light:
                App.Current.RequestedTheme = ApplicationTheme.Light;
                break;

            case Theme.Dark:
                App.Current.RequestedTheme = ApplicationTheme.Dark;
                break;
        }

        _options.UI.PropertyChanging += UIOptions_PropertyChanging;
    }

    private IServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        services.AddSingleton(_options.Core);
        services.AddSingleton(_options.UI);
        services.AddSingleton(_options);

        services.AddTransient<HighResolutionWaiter>();

        services.AddSingleton<ActionCollection>();
        services.AddSingleton<Recorder>((s) =>
        {
            var windowProps = s.GetRequiredService<WindowProperties>();
            return new(s.GetRequiredService<CoreOptions>(), s.GetRequiredService<ActionCollection>(), () => windowProps.Handle);
        });
        services.AddSingleton<Player>();

        services.AddSingleton<PathWindowService>();
        services.AddSingleton<DrawablePathWindowService>();
        services.AddSingleton<IDialogService, ContentDialogService>();
        services.AddSingleton<IFilePicker, FilePicker>();
        services.AddSingleton<IDispatcher, WinUIDispatcher>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<OptionsViewModel>();
        services.AddSingleton<ActionListViewModel>();

        services.AddSingleton<EditActionViewModelFactory>();

        services.AddSingleton<AddActionMenuItems>();

        services.AddSingleton<MainWindow>();
        services.AddSingleton<HomeView>();
        services.AddSingleton<OptionsView>();
        services.AddSingleton<HomeRibbon>();
        services.AddSingleton<AddRibbon>();
        services.AddSingleton<ActionListView>();

        services.AddSingleton<WindowProperties>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = Services.GetRequiredService<MainWindow>();

        ((FrameworkElement)_mainWindow.Content).Loaded += static async (_, _) =>
        {
            if (Current._loadingOptionsException is not null)
            {
                await Current._dialogService.ShowErrorDialog("Could not load options", Current._loadingOptionsException.Message);
            }
        };

        _mainWindow.Closed += MainWindow_Closed;

        _mainWindow.Activate();
    }

    private async void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        if (Services.GetService<PathWindowService>() is { IsPathWindowOpen: true } pathWindowService) pathWindowService.CloseWindow();

        if (!_saveOnExit) return;

        await SaveOptions();

        if (Services is IAsyncDisposable asyncDisposableServices) await asyncDisposableServices.DisposeAsync();
        else if (Services is IDisposable disposableServices) disposableServices.Dispose();
    }

    private void UIOptions_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
    {
        if (nameof(_options.UI.OptionsFileLocation).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            if (_optsFileLocReverter?.IsReverting == true) return;
            if (_optsFileLocReverter is null)
            {
                _optsFileLocReverter = new(_options.UI.OptionsFileLocation,
                                           () => _options.UI.OptionsFileLocation,
                                           (val) => _options.UI.OptionsFileLocation = val,
                                           _dispatcher);
            }
            else
            {
                _optsFileLocReverter.PreviousValue = _options.UI.OptionsFileLocation;
            }

            switch (_options.UI.OptionsFileLocation)
            {
                case OptionsFileLocation.AppData:
                    DeleteAppDataOptionsFile();
                    break;

                case OptionsFileLocation.AppFolder:
                    DeleteAppFolderOptionsFile();
                    break;
            }
        }
        else if (nameof(_options.UI.Theme).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            if (_themeOptionReverter?.IsReverting == true) return;
            if (_themeOptionReverter is null)
            {
                _themeOptionReverter = new(_options.UI.Theme,
                                           () => _options.UI.Theme,
                                           (val) => _options.UI.Theme = val,
                                           _dispatcher);
            }
            else
            {
                _themeOptionReverter.PreviousValue = _options.UI.Theme;
            }

            _ = _dialogService.ShowYesNoDialog(
                "Restart required to change theme",
                "Restart?",
                onYesClick: RestartAndChangeTheme,
                onNoClick: _themeOptionReverter.Revert);
        }
    }
    private void DeleteAppDataOptionsFile()
    {
        if (!File.Exists(Path.Combine(AppDataOptionsDir, OptionsFileName))) return;

        _ = _dialogService.ShowYesNoDialog(
            "Options file in AppData will be deleted",
            "Are you sure you want to change the options file location?",
            onYesClick: static () =>
            {
                try
                {
                    File.Delete(Path.Combine(AppDataOptionsDir, OptionsFileName));
                }
                // no need to catch FileNotFoundException because "If the file to be deleted does not exist, no exception is thrown."
                catch (DirectoryNotFoundException) { }

                try
                {
                    Directory.Delete(AppDataOptionsDir);
                }
                catch (DirectoryNotFoundException) { }
                catch (IOException) { }
            },
            onNoClick: _optsFileLocReverter!.Revert);
    }
    private void DeleteAppFolderOptionsFile()
    {
        if (!File.Exists(Path.Combine(AppContext.BaseDirectory, OptionsFileName))) return;

        _ = _dialogService.ShowYesNoDialog(
            "Options file in app folder will be deleted",
            "Are you sure you want to change the options file location?",
            onYesClick: static () =>
            {
                try
                {
                    File.Delete(Path.Combine(AppContext.BaseDirectory, OptionsFileName));
                }
                // no need to catch FileNotFoundException because "If the file to be deleted does not exist, no exception is thrown."
                catch (DirectoryNotFoundException) { }
            },
            onNoClick: _optsFileLocReverter!.Revert);
    }

    // Intentional async void, because the message dialog requires void return type and app will close anyway, so it doesnt matter.
    private async void RestartAndChangeTheme()
    {
        while (_options.UI.Theme == _themeOptionReverter!.PreviousValue) { }

        string path = Path.ChangeExtension(System.Reflection.Assembly.GetEntryAssembly()!.Location, ".exe");

        await SaveOptions();
        _saveOnExit = false;

        if (_options.UI.Theme == Theme.WindowsSetting || _options.UI.OptionsFileLocation != OptionsFileLocation.None)
        {
            Process.Start(path);
        }
        else
        {
            Process.Start(path, _options.UI.Theme == Theme.Light ? "--theme=light" : "--theme=dark");
        }
        Application.Current.Exit();

        // throws FileNotFoundException
        //Microsoft.Windows.AppLifecycle.AppInstance.Restart(UIOptions.Instance.Theme == Theme.Light ? "--theme=light" : "--theme=dark");
    }

    private async Task SaveOptions()
    {
        try
        {
            string path;

            switch (_options.UI.OptionsFileLocation)
            {
                case OptionsFileLocation.AppData:
                    path = Path.Combine(AppDataOptionsDir, OptionsFileName);
                    Directory.CreateDirectory(AppDataOptionsDir);
                    break;

                case OptionsFileLocation.AppFolder:
                    path = Path.Combine(AppContext.BaseDirectory, OptionsFileName);
                    break;

                case OptionsFileLocation.None:
                    return;

                default:
                    throw new InvalidOperationException($"{nameof(_options.UI.OptionsFileLocation)} contains an invalid value.");
            }

            await SerializationHelper.SerializeToFileAsync(_options, path, AppOptionsJsonContext.Default.AppOptions);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorDialog("Could not save options.", ex.Message);
        }
    }

    private bool TryLoadOptions(string path, [NotNullWhen(true)] out AppOptions? options)
    {
        try
        {
            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (DirectoryNotFoundException)
            {
                options = null;
                return false;
            }
            catch (FileNotFoundException)
            {
                options = null;
                return false;
            }

            options = JsonSerializer.Deserialize(json, AppOptionsJsonContext.Default.AppOptions) ?? throw new UnreachableException();

            return true;
        }
        catch (Exception ex)
        {
            _loadingOptionsException = ex;
            options = null;
            return false;
        }
    }

    /// <summary>
    /// Win32 uses pixels and WinUI 3 uses effective pixels, so this method returns the dpi scale factor.
    /// </summary>
    public static float GetWindowScalingFactor(nint hwnd)
    {
        uint dpi = PInvoke.GetDpiForWindow(hwnd);
        return dpi / 96f;
    }

    public static void SetWindowSize(nint hwnd, int width, int height)
    {
        float scalingFactor = GetWindowScalingFactor(hwnd);
        width = (int)(width * scalingFactor);
        height = (int)(height * scalingFactor);

        PInvoke.SetWindowPos(hwnd, SpecialWindowHandles.HWND_TOP, 0, 0, width, height, SetWindowPosFlags.NOMOVE);
    }
}
