using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI;

public sealed class UIOptions : ObservableObject
{
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
