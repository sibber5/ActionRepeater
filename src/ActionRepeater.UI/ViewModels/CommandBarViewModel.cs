using System;

namespace ActionRepeater.UI.ViewModels;

public class CommandBarViewModel : ViewModelBase
{
    public CmdBarHomeViewModel HomeViewModel { get; }

    public CmdBarOptionsViewModel OptionsViewModel { get; }

    public CommandBarViewModel(Func<string, string?, System.Threading.Tasks.Task> showContentDialog)
    {
        HomeViewModel = new(showContentDialog);
        OptionsViewModel = new();
    }
}
