namespace ActionRepeater.UI.ViewModels;

public class CommandBarViewModel : ViewModelBase
{
    public CmdBarHomeViewModel HomeViewModel { get; }

    public CmdBarOptionsViewModel OptionsViewModel { get; }

    public CommandBarViewModel()
    {
        HomeViewModel = new();
        OptionsViewModel = new();
    }
}
