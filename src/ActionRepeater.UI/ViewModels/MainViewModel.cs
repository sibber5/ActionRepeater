using System;
using ActionRepeater.UI.Services;

namespace ActionRepeater.UI.ViewModels;

public class MainViewModel
{
    public ActionListViewModel ActionListViewModel { get; }

    public CommandBarViewModel CommandBarViewModel { get; }

    public MainViewModel(Func<string, string?, System.Threading.Tasks.Task> showContentDialog, PathWindowService pathWindowService)
    {
        ActionListViewModel = new(showContentDialog);
        CommandBarViewModel = new(showContentDialog, pathWindowService);
    }
}
