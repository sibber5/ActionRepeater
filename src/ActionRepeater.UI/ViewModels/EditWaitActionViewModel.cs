using ActionRepeater.Core.Action;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.ViewModels;

public partial class EditWaitActionViewModel : ObservableObject
{
    [ObservableProperty]
    private double _durationSecs;

    public EditWaitActionViewModel() { }

    public EditWaitActionViewModel(WaitAction waitAction)
    {
        _durationSecs = waitAction.Duration / 1000.0;
    }
}
