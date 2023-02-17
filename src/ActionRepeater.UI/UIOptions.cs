using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI;

public sealed partial class UIOptions : ObservableObject
{
    /// <summary>
    /// For deserialization only. will be private once https://github.com/dotnet/runtime/issues/31511 is implemented.
    /// </summary>
    public UIOptions() { }
    private static UIOptions? _instance;
    public static UIOptions Instance => _instance ??= new();

    // String.Text.Json source-gen doesnt seem to be able to detect source-genned properties,
    // so we manually call SetProperty

    private Theme _theme = Theme.WindowsSetting;
    public Theme Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }

    private OptionsFileLocation _optionsFileLocation = OptionsFileLocation.None;
    public OptionsFileLocation OptionsFileLocation
    {
        get => _optionsFileLocation;
        set => SetProperty(ref _optionsFileLocation, value);
    }

    public static void Load(UIOptions options)
    {
        _instance = options;
    }
}

public enum Theme
{
    WindowsSetting = 0,
    Light,
    Dark,
}

public enum OptionsFileLocation
{
    None,
    AppData,
    AppFolder,
}
