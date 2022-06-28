using System;
using ActionRepeater.UI.Services;
using ActionRepeater.UI.Utilities;

namespace ActionRepeater.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ActionListViewModel ActionListViewModel { get; }

    public CommandBarViewModel CommandBarViewModel { get; }

    public MainViewModel(ActionHolder copiedActionHolder, Func<string, string?, System.Threading.Tasks.Task> showContentDialog, PathWindowService pathWindowService)
    {
        ActionListViewModel = new(copiedActionHolder, showContentDialog);
        CommandBarViewModel = new(showContentDialog, pathWindowService);
    }
}
