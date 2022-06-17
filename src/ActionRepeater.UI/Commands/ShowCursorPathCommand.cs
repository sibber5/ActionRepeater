using System;
using ActionRepeater.Core.Input;

namespace ActionRepeater.UI.Commands;

public class ShowCursorPathCommand : CommandBase
{
    public ShowCursorPathCommand()
    {
        ActionManager.CursorPathStartChanged += (_, _) => RaiseCanExecuteChanged();
    }

    public override bool CanExecute(object? parameter) => ActionManager.CursorPathStart is not null;

    public override void Execute(object? parameter)
    {
        // TODO: implement ShowCursorPathCommand.Execute
        throw new NotImplementedException();
    }
}
