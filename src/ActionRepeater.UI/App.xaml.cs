using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ActionRepeater.Core.Helpers;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Factories;
using ActionRepeater.UI.Services;
using ActionRepeater.UI.Utilities;
using ActionRepeater.UI.ViewModels;
using ActionRepeater.Win32;
using ActionRepeater.Win32.Synch.Utilities;
using ActionRepeater.Win32.WindowsAndMessages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public static new App Current => (App)Application.Current;

    public static string AppDataOptionsDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(ActionRepeater));
    public static string OptionsFileName => "Options.json";

    public MainWindow MainWindow { get; private set; } = null!;

    public IServiceProvider Services { get; }

    private readonly ContentDialogService _contentDialogService;

    private bool _saveOnExit = true;

    private Exception? _loadingOptionsException;

    private ObservablePropertyReverter<OptionsFileLocation>? _optsFileLocReverter;

    // used to check what UIOptions.Theme was changed from, when its changed
    private readonly bool _wasAppThemeSetOnStartup;

    private bool _isRevertingTheme;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        Services = ConfigureServices();

        _contentDialogService = Services.GetRequiredService<ContentDialogService>();

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
                _wasAppThemeSetOnStartup = true;
                break;

            case Theme.Dark:
                App.Current.RequestedTheme = ApplicationTheme.Dark;
                _wasAppThemeSetOnStartup = true;
                break;
        }

        UIOptions.Instance.PropertyChanging += UIOptions_PropertyChanging;
        UIOptions.Instance.PropertyChanged += UIOptions_PropertyChanged;
    }

    private static IServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        services.AddTransient<HighResolutionWaiter>();

        services.AddSingleton<ActionCollection>();
        services.AddSingleton<Recorder>();
        services.AddSingleton<Player>();

        services.AddSingleton<PathWindowService>();
        services.AddSingleton<ContentDialogService>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ActionListViewModel>();
        services.AddSingleton<HomePageViewModel>();
        services.AddSingleton<OptionsPageViewModel>();

        services.AddSingleton<EditActionViewModelFactory>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new();

        // XamlRoot has to be set after Content has loaded
        ((FrameworkElement)MainWindow.Content).Loaded += static async (_, _) =>
        {
            Current._contentDialogService.XamlRoot = App.Current.MainWindow.GridXamlRoot;
            System.Diagnostics.Debug.WriteLine("XamlRoot set.");

            if (Current._loadingOptionsException is not null)
            {
                await Current._contentDialogService.ShowErrorDialog("Could not load options", Current._loadingOptionsException.Message);
            }
        };

        MainWindow.Closed += MainWindow_Closed;

        MainWindow.Activate();
    }

    private async void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        if (Services.GetService<PathWindowService>() is { IsPathWindowOpen: true } pathWindowService) pathWindowService.ClosePathWindow();

        if (!_saveOnExit) return;

        await SaveOptions();

        if (Services is IAsyncDisposable asyncDisposableServices) await asyncDisposableServices.DisposeAsync();
        else if (Services is IDisposable disposableServices) disposableServices.Dispose();
    }

    private async void UIOptions_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
    {
        if (!nameof(UIOptions.Instance.OptionsFileLocation).Equals(e.PropertyName, StringComparison.Ordinal)) return;
        
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
                {
                    string dirPath = AppDataOptionsDir;
                    string filePath = Path.Combine(dirPath, OptionsFileName);

                    if (!File.Exists(filePath)) break;

                    ContentDialogResult dialogResult = await _contentDialogService.ShowYesNoMessageDialog(
                        "Options file in AppData will be deleted",
                        "Are you sure you want to change the options file location?");

                    switch (dialogResult)
                    {
                        case ContentDialogResult.Primary:
                            try
                            {
                                File.Delete(filePath);
                            }
                            // no need to catch FileNotFoundException because "If the file to be deleted does not exist, no exception is thrown."
                            catch (DirectoryNotFoundException) { }

                            try
                            {
                                Directory.Delete(dirPath);
                            }
                            catch (DirectoryNotFoundException) { }
                            catch (IOException) { }
                            break;

                        case ContentDialogResult.Secondary:
                            _optsFileLocReverter.Revert();
                            break;
                    }
                }
                break;

            case OptionsFileLocation.AppFolder:
                {
                    string filePath = Path.Combine(AppContext.BaseDirectory, OptionsFileName);

                    if (!File.Exists(filePath)) break;

                    ContentDialogResult dialogResult = await _contentDialogService.ShowYesNoMessageDialog(
                        "Options file in app folder will be deleted",
                        "Are you sure you want to change the options file location?");

                    switch (dialogResult)
                    {
                        case ContentDialogResult.Primary:
                            try
                            {
                                File.Delete(filePath);
                            }
                            // no need to catch FileNotFoundException because "If the file to be deleted does not exist, no exception is thrown."
                            catch (DirectoryNotFoundException) { }
                            break;

                        case ContentDialogResult.Secondary:
                            _optsFileLocReverter.Revert();
                            break;
                    }
                }
                break;
        }
    }

    private async void UIOptions_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (!nameof(UIOptions.Instance.Theme).Equals(e.PropertyName, StringComparison.Ordinal)) return;

        if (_isRevertingTheme) return;

        Action revertThemeOption = static () =>
        {
            Current._isRevertingTheme = true;
            Current.MainWindow.DispatcherQueue.TryEnqueue(static () =>
            {
                Theme previousTheme;
                if (Current._wasAppThemeSetOnStartup)
                {
                    previousTheme = UIOptions.Instance.Theme == Theme.Light ? Theme.Dark : Theme.Light;
                }
                else
                {
                    previousTheme = Theme.WindowsSetting;
                }

                UIOptions.Instance.Theme = previousTheme;
                Current._isRevertingTheme = false;
            });
        };

        ContentDialogResult dialogResult = await _contentDialogService.ShowYesNoMessageDialog("Restart required to change theme", "Restart?");

        switch (dialogResult)
        {
            case ContentDialogResult.Primary:
                string path = Path.ChangeExtension(System.Reflection.Assembly.GetEntryAssembly()!.Location, ".exe");

                await SaveOptions();
                _saveOnExit = false;

                if (UIOptions.Instance.Theme == Theme.WindowsSetting || UIOptions.Instance.OptionsFileLocation != OptionsFileLocation.None)
                {
                    System.Diagnostics.Process.Start(path);
                }
                else
                {
                    System.Diagnostics.Process.Start(path, UIOptions.Instance.Theme == Theme.Light ? "--theme=light" : "--theme=dark");
                }
                Application.Current.Exit();

                // throws FileNotFoundException
                //Microsoft.Windows.AppLifecycle.AppInstance.Restart(UIOptions.Instance.Theme == Theme.Light ? "--theme=light" : "--theme=dark");
                break;

            case ContentDialogResult.Secondary:
                revertThemeOption();
                break;
        }
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
    public static float GetWindowScalingFactor(IntPtr hwnd)
    {
        uint dpi = PInvoke.GetDpiForWindow(hwnd);
        return dpi / 96f;
    }

    public static void SetWindowSize(IntPtr hwnd, int width, int height)
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
