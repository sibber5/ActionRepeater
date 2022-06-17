using ActionRepeater.UI.Utilities;
using ActionRepeater.UI.ViewModels;

namespace ActionRepeater.UI.Commands;

public class StoreActionCommand : CommandBase
{
    private readonly ActionHolder _holder;
    private readonly ActionListViewModel _listViewModel;

    public StoreActionCommand(ActionHolder holder, ActionListViewModel listViewModel)
    {
        _holder = holder;
        _listViewModel = listViewModel;
    }

    public override void Execute(object? parameter) => _holder.Action = _listViewModel.SelectedAction;
}
