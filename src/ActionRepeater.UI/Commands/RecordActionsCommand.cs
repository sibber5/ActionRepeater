using ActionRepeater.Core.Input;

namespace ActionRepeater.UI.Commands;

public class RecordActionsCommand : CommandBase
{
    public RecordActionsCommand()
    {
        Player.IsPlayingChanged += (_, _) => RaiseCanExecuteChanged();
    }

    public override bool CanExecute(object? parameter) => !Player.IsPlaying;

    public override void Execute(object? parameter)
    {
        if (Recorder.IsRecording)
        {
            Recorder.StopRecording();
            return;
        }

        if (!Recorder.IsSubscribed) Recorder.RegisterRawInput(App.MainWindow.Handle);

        Recorder.StartRecording();
    }
}
