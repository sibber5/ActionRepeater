using System.Collections.Specialized;
using ActionRepeater.Core.Input;

namespace ActionRepeater.UI.Commands;

public class PlayActionsCommand : CommandBase
{
    public PlayActionsCommand()
    {
        Recorder.IsRecordingChanged += (_, _) => RaiseCanExecuteChanged();
        ActionManager.ActionCollectionChanged += Actions_CollectionChanged;
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

    public override bool CanExecute(object? parameter) => !Recorder.IsRecording && ActionManager.Actions.Count > 0;

    public override void Execute(object? parameter)
    {
        if (!ActionManager.TryPlayActions())
        {
            Player.RefreshIsPlaying();
        }
    }
}
