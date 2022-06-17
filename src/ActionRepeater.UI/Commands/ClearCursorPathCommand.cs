using ActionRepeater.Core.Input;

namespace ActionRepeater.UI.Commands;

public class ClearCursorPathCommand : CommandBase
{
    public ClearCursorPathCommand()
    {
        ActionManager.CursorPathStartChanged += ActionManager_CursorPathStartChanged;
    }

    private void ActionManager_CursorPathStartChanged(object? sender, Core.Action.MouseMovement? e) => RaiseCanExecuteChanged();

    public override bool CanExecute(object? parameter) => ActionManager.CursorPathStart is not null;

    public override void Execute(object? parameter) => ActionManager.ClearCursorPath();
}
