using ActionRepeater.Core.Input;
using ActionRepeater.UI.Utilities;
using ActionRepeater.UI.ViewModels;

namespace ActionRepeater.UI.Commands;

public class ReplaceActionCommand : CommandBase
{
    private readonly ActionHolder _holder;
    private readonly ActionListViewModel _listViewModel;

    public ReplaceActionCommand(ActionHolder holder, ActionListViewModel listViewModel)
    {
        _holder = holder;
        _listViewModel = listViewModel;
        _holder.ActionChanged += ActionHolder_ActionChanged;
    }

    private void ActionHolder_ActionChanged(object? sender, System.EventArgs e) => RaiseCanExecuteChanged();

    public override bool CanExecute(object? parameter) => _holder.Action is not null;

    public override void Execute(object? parameter)
    {
        ActionManager.ReplaceAction(!_listViewModel.ShowKeyRepeatActions, _listViewModel.SelectedActionIndex, _holder.Action!.Clone());
    }
}
