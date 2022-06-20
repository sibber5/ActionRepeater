using System;
using System.Windows.Input;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Commands;

namespace ActionRepeater.UI.ViewModels;

public class CmdBarHomeViewModel : ViewModelBase
{
    public bool IsPlayButtonChecked => Player.IsPlaying;

    public ICommand RecordCommand { get; }
    public ICommand PlayCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand ShowCursorPathCommand { get; }

    public CmdBarHomeViewModel(Func<string, string?, System.Threading.Tasks.Task> showContentDialog)
    {
        RecordCommand = new RecordActionsCommand();
        PlayCommand = new PlayActionsCommand();
        ExportCommand = new ExportActionsCommand(showContentDialog);
        ImportCommand = new ImportActionsCommand(showContentDialog);
        ShowCursorPathCommand = new ShowCursorPathCommand();

        Player.IsPlayingChanged += (_, _) => RaisePropertyChanged(nameof(IsPlayButtonChecked));
    }
}
