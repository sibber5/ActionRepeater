using ActionRepeater.UI.Services;

namespace ActionRepeater.UI.ViewModels;

public class MainViewModel
{
    public ActionListViewModel ActionListViewModel { get; }

    public CommandBarViewModel CommandBarViewModel { get; }

    public MainViewModel(ContentDialogService contentDialogService, PathWindowService pathWindowService)
    {
        ActionListViewModel = new(contentDialogService);
        CommandBarViewModel = new(contentDialogService, pathWindowService, ActionListViewModel);
    }
}
