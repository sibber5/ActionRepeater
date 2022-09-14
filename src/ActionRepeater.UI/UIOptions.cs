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

    [ObservableProperty]
    private Theme _theme = Theme.WindowsSetting;

    [ObservableProperty]
    private OptionsFileLocation _optionsFileLocation = OptionsFileLocation.None;

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
