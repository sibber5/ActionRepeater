using ActionRepeater.Core.Action;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.ViewModels;

public sealed partial class EditWaitActionViewModel : ObservableObject
{
    [ObservableProperty]
    private double _durationSecs;

    public EditWaitActionViewModel() { }

    public EditWaitActionViewModel(WaitAction waitAction)
    {
        _durationSecs = waitAction.DurationMS / 1000.0;
    }
}
