using ActionRepeater.Core.Input;
using ActionRepeater.UI.Utilities;

namespace ActionRepeater.UI.Commands;

public class AddActionCommand : CommandBase
{
    private readonly ActionHolder _holder;

    public AddActionCommand(ActionHolder holder)
    {
        _holder = holder;
        _holder.ActionChanged += ActionHolder_ActionChanged;
    }

    private void ActionHolder_ActionChanged(object? sender, System.EventArgs e) => RaiseCanExecuteChanged();

    public override bool CanExecute(object? parameter) => _holder.Action is not null;

    public override void Execute(object? parameter) => ActionManager.AddAction(_holder.Action!.Clone());
}
