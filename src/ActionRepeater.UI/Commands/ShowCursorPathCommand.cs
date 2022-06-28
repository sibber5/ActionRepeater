using ActionRepeater.Core.Input;
using ActionRepeater.UI.Services;

namespace ActionRepeater.UI.Commands;

public class ShowCursorPathCommand : CommandBase
{
    private readonly PathWindowService _pathWindowService;

    public ShowCursorPathCommand(PathWindowService pathWindowService)
    {
        _pathWindowService = pathWindowService;
        //ActionManager.CursorPathStartChanged += (_, _) => RaiseCanExecuteChanged();
    }

    //public override bool CanExecute(object? parameter) => ActionManager.CursorPathStart is not null;

    public override void Execute(object? parameter)
    {
        if (_pathWindowService.IsPathWindowOpen)
        {
            _pathWindowService.ClosePathWindow();
        }
        else
        {
            _pathWindowService.OpenPathWindow();
        }
    }
}
