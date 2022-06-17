using ActionRepeater.UI.Utilities;

namespace ActionRepeater.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ActionListViewModel ActionListViewModel { get; }

    public CommandBarViewModel CommandBarViewModel { get; }

    public MainViewModel(ActionHolder copiedActionHolder)
    {
        ActionListViewModel = new(copiedActionHolder);
        CommandBarViewModel = new();
    }
}
