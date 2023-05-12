using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ActionRepeater.Core;
using ActionRepeater.Core.Extentions;
using ActionRepeater.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.ViewModels;

public sealed class OptionsPageViewModel : ObservableObject
{
    public int MaxClickInterval
    {
        get => _options.Core.MaxClickInterval;
        set => _options.Core.MaxClickInterval = value;
    }

    public bool SendKeyAutoRepeat
    {
        get => _options.Core.SendKeyAutoRepeat;
        set => _options.Core.SendKeyAutoRepeat = value;
    }

    public int CursorMovementMode
    {
        get => (int)_options.Core.CursorMovementMode;
        set => _options.Core.CursorMovementMode = (CursorMovementMode)value;
    }

    public bool DisplayAccelerationWarning => _options.Core.CursorMovementMode == Core.CursorMovementMode.Absolute && Win32.SystemInformation.IsMouseAccelerationEnabled;

    public bool UseCursorPosOnClicks
    {
        get => _options.Core.UseCursorPosOnClicks;
        set => _options.Core.UseCursorPosOnClicks = value;
    }

    public bool SendAbsoluteCursorCoords
    {
        get => _options.Core.SendAbsoluteCursorCoords;
        set => _options.Core.SendAbsoluteCursorCoords = value;
    }

    public int Theme
    {
        get => (int)_options.UI.Theme;
        set => _options.UI.Theme = (Theme)value;
    }

    public int OptionsFileLocation
    {
        get => (int)_options.UI.OptionsFileLocation;
        set => _options.UI.OptionsFileLocation = (OptionsFileLocation)value;
    }

    public IEnumerable<string> CursorMovementCBItems => Enum.GetNames<CursorMovementMode>().Select(x => x.AddSpacesBetweenWords());

    public IEnumerable<string> ThemeCBItems => Enum.GetNames<Theme>().Select(x => x.AddSpacesBetweenWords());


    private readonly PropertyChangedEventArgs _isCursorMovementModeChangedArgs = new(nameof(IsCursorMovementMode));
    private readonly PropertyChangedEventArgs _displayAccelerationWarningChangedArgs = new(nameof(DisplayAccelerationWarning));
    
    private readonly AppOptions _options;

    public OptionsPageViewModel(AppOptions options)
    {
        _options = options;

        PropertyChangedEventHandler callPropChange = (s, e) =>
        {
            OnPropertyChanged(e);
            if (nameof(CursorMovementMode).Equals(e.PropertyName, StringComparison.Ordinal))
            {
                OnPropertyChanged(_isCursorMovementModeChangedArgs);
                OnPropertyChanged(_displayAccelerationWarningChangedArgs);
            }
        };
        _options.Core.PropertyChanged += callPropChange;
        _options.UI.PropertyChanged += callPropChange;
    }

    public bool IsCursorMovementMode(CursorMovementMode mode) => _options.Core.CursorMovementMode == mode;
}
