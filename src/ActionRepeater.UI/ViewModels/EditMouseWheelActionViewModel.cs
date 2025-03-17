using ActionRepeater.Core.Action;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.ViewModels;

public sealed partial class EditMouseWheelActionViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _horizontalScrolling;

    [ObservableProperty]
    private int _steps;

    [ObservableProperty]
    private double _durationSecs;

    public EditMouseWheelActionViewModel() { }

    public EditMouseWheelActionViewModel(MouseWheelAction mouseWheelAction)
    {
        _horizontalScrolling = mouseWheelAction.IsHorizontal;
        _steps = mouseWheelAction.StepCount;
        _durationSecs = mouseWheelAction.DurationMS / 1000.0;
    }
}
