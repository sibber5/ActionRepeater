using System;
using ActionRepeater.UI.Services;

namespace ActionRepeater.UI.ViewModels;

public class CommandBarViewModel
{
    public CmdBarHomeViewModel HomeViewModel { get; }

    public CmdBarOptionsViewModel OptionsViewModel { get; }

    public CommandBarViewModel(Func<string, string?, System.Threading.Tasks.Task> showContentDialog, PathWindowService pathWindowService)
    {
        HomeViewModel = new(showContentDialog, pathWindowService);
        OptionsViewModel = new();
    }
}
