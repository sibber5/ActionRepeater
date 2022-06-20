using System;
using System.Threading.Tasks;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.ViewModels;

namespace ActionRepeater.UI.Commands;

public class RemoveActionCommand : CommandBase
{
    private readonly ActionListViewModel _listViewModel;
    private readonly Func<string, string?, Task> _showContentDialog;

    public RemoveActionCommand(ActionListViewModel listViewModel, Func<string, string?, Task> showContentDialog)
    {
        _listViewModel = listViewModel;
        _showContentDialog = showContentDialog;
    }

    public override async void Execute(object? parameter)
    {
        if (!ActionManager.TryRemoveAction(_listViewModel.SelectedAction!))
        {
            await _showContentDialog(
                "❌ Failed to remove action",
                $"This action represents multiple hidden actions (because \"{nameof(_listViewModel.ShowKeyRepeatActions)}\" is off).{Environment.NewLine}Removing it will result in unexpected behavior.");
        }
    }
}
