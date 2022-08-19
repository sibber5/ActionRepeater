using System;
using System.Collections.Generic;
using System.Linq;
using ActionRepeater.Core;
using ActionRepeater.Core.Extentions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.ViewModels;

public class OptionsPageViewModel : ObservableObject
{
    public int CursorMovementMode
    {
        get => (int)Options.Instance.CursorMovementMode;
        set => Options.Instance.CursorMovementMode = (CursorMovementMode)value;
    }

    public bool UseCursorPosOnClicks
    {
        get => Options.Instance.UseCursorPosOnClicks;
        set => Options.Instance.UseCursorPosOnClicks = value;
    }

    public int MaxClickInterval
    {
        get => Options.Instance.MaxClickInterval;
        set => Options.Instance.MaxClickInterval = value;
    }

    public bool SendKeyAutoRepeat
    {
        get => Options.Instance.SendKeyAutoRepeat;
        set => Options.Instance.SendKeyAutoRepeat = value;
    }

    public int Theme
    {
        get => (int)UIOptions.Instance.Theme;
        set => UIOptions.Instance.Theme = (Theme)value;
    }

    public int OptionsFileLocation
    {
        get => (int)UIOptions.Instance.OptionsFileLocation;
        set => UIOptions.Instance.OptionsFileLocation = (OptionsFileLocation)value;
    }

    public IEnumerable<string> CursorMovementCBItems => Enum.GetNames<CursorMovementMode>().Select(x => x.AddSpacesBetweenWords());

    public IEnumerable<string> ThemeCBItems => Enum.GetNames<Theme>().Select(x => x.AddSpacesBetweenWords());

    public OptionsPageViewModel()
    {
        System.ComponentModel.PropertyChangedEventHandler callPropChange = (s, e) => OnPropertyChanged(e);
        Options.Instance.PropertyChanged += callPropChange;
        UIOptions.Instance.PropertyChanged += callPropChange;
    }
}
