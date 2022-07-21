using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.ViewModels;

public partial class EditMouseWheelActionViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _horizontalScrolling;

    [ObservableProperty]
    private int _steps;

    [ObservableProperty]
    private double _durationSecs;
}
