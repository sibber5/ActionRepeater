using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.ViewModels;

public partial class EditWaitActionViewModel : ObservableObject
{
    [ObservableProperty]
    private double _durationSecs;
}
