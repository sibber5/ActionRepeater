using System.Collections.Specialized;
using ActionRepeater.Core.Input;

namespace ActionRepeater.UI.Commands;

public class ClearActionsAndCursorPathCommand : CommandBase
{
    public ClearActionsAndCursorPathCommand()
    {
        ActionManager.Actions.CollectionChanged += Actions_CollectionChanged;
        ActionManager.CursorPathStartChanged += ActionManager_CursorPathStartChanged;
    }

    private void Actions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add
            || e.Action == NotifyCollectionChangedAction.Remove
            || e.Action == NotifyCollectionChangedAction.Reset)
        {
            RaiseCanExecuteChanged();
        }
    }

    private void ActionManager_CursorPathStartChanged(object? sender, Core.Action.MouseMovement? e) => RaiseCanExecuteChanged();

    public override bool CanExecute(object? parameter) => ActionManager.Actions.Count > 0 || ActionManager.CursorPathStart is not null;

    public override void Execute(object? parameter) => ActionManager.ClearAll();
}
