using System;
using System.IO;
using System.Text.Json;
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
    public static string AppFolderDir => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!;
    public static string OptionsFileName => "Options.json";

    public MainWindow MainWindow { get; private set; } = null!;

    public IServiceProvider Services { get; }

    private readonly ContentDialogService _contentDialogService;

    private bool _saveOptions;
    private bool _saveOnExit = true;

    private Exception? _loadingOptionsException;

    private ObservablePropertyReverter<OptionsFileLocation>? _optsFileLocReverter;

    // used to check what UIOptions.Theme was changed from, when its changed
    private bool _wasAppThemeSet = false;

    private bool _isRevertingTheme = false;

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
            TryLoadOptions(Path.Combine(AppFolderDir, OptionsFileName));
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

        services.AddSingleton<EditActionDialogViewModelFactory>();

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
        if (Services.GetService<PathWindowService>() is { } pathWindowService && pathWindowService.IsPathWindowOpen) pathWindowService.ClosePathWindow();

        if (!_saveOnExit) return;

        await SaveOptions();

        if (Services is IAsyncDisposable asyncDisposableServices) await asyncDisposableServices.DisposeAsync();
        else if (Services is IDisposable disposableServices) disposableServices.Dispose();
    }

    private async void UIOptions_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
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
                    {
                        if (!Directory.Exists(AppDataOptionsDir)) break;

                        ContentDialogResult dialogResult = await _contentDialogService.ShowYesNoMessageDialog(
                            "Options file in AppData will be deleted",
                            "Are you sure you want to change the options file location?");

                        switch (dialogResult)
                        {
                            case ContentDialogResult.Primary:
                                try
                                {
                                    Directory.Delete(AppDataOptionsDir, true);
                                }
                                catch (DirectoryNotFoundException) { }
                                break;

                            case ContentDialogResult.Secondary:
                                _optsFileLocReverter.Revert();
                                break;
                        }
                    }
                    break;

                case OptionsFileLocation.AppFolder:
                    {
                        string path = Path.Combine(AppFolderDir, OptionsFileName);

                        if (!File.Exists(path)) break;

                        ContentDialogResult dialogResult = await _contentDialogService.ShowYesNoMessageDialog(
                            "Options file in app folder will be deleted",
                            "Are you sure you want to change the options file location?");

                        switch (dialogResult)
                        {
                            case ContentDialogResult.Primary:
                                try
                                {
                                    File.Delete(path);
                                }
                                catch (DirectoryNotFoundException) { }
                                // no need to catch FileNotFoundException because "If the file to be deleted does not exist, no exception is thrown."
                                break;

                            case ContentDialogResult.Secondary:
                                _optsFileLocReverter.Revert();
                                break;
                        }
                    }
                    break;
            }
        }
    }

    private async void UIOptions_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(UIOptions.Instance.Theme):
                if (_isRevertingTheme) break;

                Action revertThemeOption = static () =>
                {
                    Current._isRevertingTheme = true;
                    Current.MainWindow.DispatcherQueue.TryEnqueue(static () =>
                    {
                        Theme previousTheme;
                        if (Current._wasAppThemeSet)
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

                        await Current.SaveOptions();
                        Current._saveOnExit = false;

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

    private async Task SaveOptions()
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
                    path = Path.Combine(AppFolderDir, OptionsFileName);
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

            AllOptions options = JsonSerializer.Deserialize<AllOptions>(json);
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

public record struct AllOptions(Core.Options CoreOptions, UIOptions UIOptions);
