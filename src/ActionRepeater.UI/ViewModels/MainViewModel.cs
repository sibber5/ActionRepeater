using ActionRepeater.UI.Services;
using ActionRepeater.UI.Utilities;

namespace ActionRepeater.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ActionListViewModel ActionListViewModel { get; }

    public CmdBarNavigationService CmdBarNavigationService { get; }

    public MainViewModel(ActionHolder copiedActionHolder, CmdBarNavigationService cmdBarNavService)
    {
        ActionListViewModel = new(copiedActionHolder);
        CmdBarNavigationService = cmdBarNavService;
    }
}
