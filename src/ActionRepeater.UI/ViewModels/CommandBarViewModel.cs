using ActionRepeater.UI.Services;

namespace ActionRepeater.UI.ViewModels;

public class CommandBarViewModel
{
    public CmdBarHomeViewModel HomeViewModel { get; }

    public CmdBarOptionsViewModel OptionsViewModel { get; }

    public CommandBarViewModel(ContentDialogService contentDialogService, PathWindowService pathWindowService, ActionListViewModel actionListVM)
    {
        HomeViewModel = new(contentDialogService, pathWindowService, actionListVM);
        OptionsViewModel = new(contentDialogService);
    }
}
