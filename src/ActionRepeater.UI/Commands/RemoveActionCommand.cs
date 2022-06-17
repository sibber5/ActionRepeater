using System;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.ViewModels;

namespace ActionRepeater.UI.Commands;

public class RemoveActionCommand : CommandBase
{
    private readonly ActionListViewModel _listViewModel;

    public RemoveActionCommand(ActionListViewModel listViewModel)
    {
        _listViewModel = listViewModel;
    }

    public override async void Execute(object? parameter)
    {
        if (true) //(!ActionManager.TryRemoveAction(_listViewModel.SelectedAction!))
        {
            if (_listViewModel.ShowErrorDialog is null) return;

            await _listViewModel.ShowErrorDialog(
                "Failed to remove action",
                $"This action represents multiple hidden actions (because \"{nameof(_listViewModel.ShowKeyRepeatActions)}\" is off).{Environment.NewLine}Removing it will result in unexpected behavior.");
        }
    }
}
