using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ActionRepeater.Core.Helpers;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Factories;
using ActionRepeater.UI.Pages;
using ActionRepeater.UI.Services;
using ActionRepeater.UI.Utilities;
using ActionRepeater.UI.ViewModels;
using ActionRepeater.UI.Views;
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

    private readonly ContentDialogService _contentDialogService;
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
        Services = ConfigureServices();

        _contentDialogService = Services.GetRequiredService<ContentDialogService>();
        _dispatcher = Services.GetRequiredService<IDispatcher>();

        InitializeComponent();

        if (!TryLoadOptions(Path.Combine(AppDataOptionsDir, OptionsFileName)))
        {
            TryLoadOptions(Path.Combine(AppContext.BaseDirectory, OptionsFileName));
        }

        string[] args = Environment.GetCommandLineArgs();
        string? themeArg = args.FirstOrDefault(static x => x.StartsWith("--theme="));
        if (themeArg is not null)
        {
            var theme = themeArg.AsSpan(8);
            UIOptions.Instance.Theme = theme switch
            {
                "light" => Theme.Light,
                "dark" => Theme.Dark,
                _ => throw new NotSupportedException(),
            };
        }

        switch (UIOptions.Instance.Theme)
        {
            case Theme.Light:
                App.Current.RequestedTheme = ApplicationTheme.Light;
                break;

            case Theme.Dark:
                App.Current.RequestedTheme = ApplicationTheme.Dark;
                break;
        }

        UIOptions.Instance.PropertyChanging += UIOptions_PropertyChanging;
    }

    private static IServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        services.AddTransient<HighResolutionWaiter>();

        services.AddSingleton<ActionCollection>();
        services.AddSingleton<Recorder>((s) =>
        {
            var windowProps = s.GetRequiredService<WindowProperties>();
            return new(s.GetRequiredService<ActionCollection>(), () => windowProps.Handle);
        });
        services.AddSingleton<Player>();

        services.AddSingleton<PathWindowService>();
        services.AddSingleton<ContentDialogService>();

        services.AddSingleton<IFilePicker, FilePicker>();
        services.AddSingleton<IDispatcher, WinUIDispatcher>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ActionListViewModel>();
        services.AddSingleton<HomePageViewModel>();
        services.AddSingleton<OptionsPageViewModel>();

        services.AddSingleton<EditActionViewModelFactory>();

        services.AddSingleton<AddActionMenuItems>();
        services.AddSingleton<HomePageParameter>();
        services.AddSingleton<OptionsPageParameter>();
        services.AddSingleton<MainWindow>();

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
                await Current._contentDialogService.ShowErrorDialog("Could not load options", Current._loadingOptionsException.Message);
            }
        };

        _mainWindow.Closed += MainWindow_Closed;

        _mainWindow.Activate();
    }

    private async void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        if (Services.GetService<PathWindowService>() is { IsPathWindowOpen: true } pathWindowService) pathWindowService.ClosePathWindow();

        if (!_saveOnExit) return;

        await SaveOptions();

        if (Services is IAsyncDisposable asyncDisposableServices) await asyncDisposableServices.DisposeAsync();
        else if (Services is IDisposable disposableServices) disposableServices.Dispose();
    }

    private void UIOptions_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
    {
        if (nameof(UIOptions.Instance.OptionsFileLocation).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            if (_optsFileLocReverter?.IsReverting == true) return;
            if (_optsFileLocReverter is null)
            {
                _optsFileLocReverter = new(UIOptions.Instance.OptionsFileLocation,
                                           static () => UIOptions.Instance.OptionsFileLocation,
                                           static (val) => UIOptions.Instance.OptionsFileLocation = val,
                                           _dispatcher);
            }
            else
            {
                _optsFileLocReverter.PreviousValue = UIOptions.Instance.OptionsFileLocation;
            }

            switch (UIOptions.Instance.OptionsFileLocation)
            {
                case OptionsFileLocation.AppData:
                    DeleteAppDataOptionsFile();
                    break;

                case OptionsFileLocation.AppFolder:
                    DeleteAppFolderOptionsFile();
                    break;
            }
        }
        else if (nameof(UIOptions.Instance.Theme).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            if (_themeOptionReverter?.IsReverting == true) return;
            if (_themeOptionReverter is null)
            {
                _themeOptionReverter = new(UIOptions.Instance.Theme,
                                           static () => UIOptions.Instance.Theme,
                                           static (val) => UIOptions.Instance.Theme = val,
                                           _dispatcher);
            }
            else
            {
                _themeOptionReverter.PreviousValue = UIOptions.Instance.Theme;
            }

            _ = _contentDialogService.ShowYesNoMessageDialog(
                "Restart required to change theme",
                "Restart?",
                onYesClick: RestartAndChangeTheme,
                onNoClick: _themeOptionReverter.Revert);
        }
    }
    private void DeleteAppDataOptionsFile()
    {
        if (!File.Exists(Path.Combine(AppDataOptionsDir, OptionsFileName))) return;

        _ = _contentDialogService.ShowYesNoMessageDialog(
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

        _ = _contentDialogService.ShowYesNoMessageDialog(
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
        while (UIOptions.Instance.Theme == _themeOptionReverter!.PreviousValue) { }

        string path = Path.ChangeExtension(System.Reflection.Assembly.GetEntryAssembly()!.Location, ".exe");

        await SaveOptions();
        _saveOnExit = false;

        if (UIOptions.Instance.Theme == Theme.WindowsSetting || UIOptions.Instance.OptionsFileLocation != OptionsFileLocation.None)
        {
            Process.Start(path);
        }
        else
        {
            Process.Start(path, UIOptions.Instance.Theme == Theme.Light ? "--theme=light" : "--theme=dark");
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

            switch (UIOptions.Instance.OptionsFileLocation)
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
                    throw new InvalidOperationException($"{nameof(UIOptions.Instance.OptionsFileLocation)} contains an invalid value.");
            }

            AllOptions options = new(Core.CoreOptions.Instance, UIOptions.Instance);
            await SerializationHelper.SerializeToFileAsync(options, path, AllOptionsJsonContext.Default.AllOptions);
        }
        catch (Exception ex)
        {
            await _contentDialogService.ShowErrorDialog("Could not save options.", ex.Message);
        }
    }

    private bool TryLoadOptions(string path)
    {
        try
        {
            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (DirectoryNotFoundException) { return false; }
            catch (FileNotFoundException) { return false; }

            AllOptions options = JsonSerializer.Deserialize(json, AllOptionsJsonContext.Default.AllOptions);
            Core.CoreOptions.Load(options.CoreOptions);
            UIOptions.Load(options.UIOptions);

            return true;
        }
        catch (Exception ex)
        {
            _loadingOptionsException = ex;
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

public record struct AllOptions(Core.CoreOptions CoreOptions, UIOptions UIOptions);

[JsonSerializable(typeof(AllOptions))]
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default, WriteIndented = true)]
public partial class AllOptionsJsonContext : JsonSerializerContext { }
