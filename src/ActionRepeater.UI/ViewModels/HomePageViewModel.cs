using System;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActionRepeater.UI.ViewModels;

public sealed partial class HomePageViewModel : ObservableObject
{
    public int PlayRepeatCount
    {
        get => Core.CoreOptions.Instance.PlayRepeatCount;
        set => Core.CoreOptions.Instance.PlayRepeatCount = value;
    }

    [ObservableProperty]
    private bool _isPlayButtonChecked;

    public bool CanAddAction => !_recorder.IsRecording;

    private readonly PathWindowService _pathWindowService;

    private readonly ActionCollection _actionCollection;
    private readonly Recorder _recorder;
    private readonly Player _player;

    private readonly Action _onIsPlayingChanged;

    public HomePageViewModel(PathWindowService pathWindowService, ActionCollection actionCollection, ActionListViewModel actionListVM, Player player, Recorder recorder, IDispatcher dispatcher)
    {
        _pathWindowService = pathWindowService;

        _actionCollection = actionCollection;
        _player = player;
        _recorder = recorder;

        _player.OnActionPlayed = actionListVM.UpdateSelectedAction;

        _onIsPlayingChanged = () =>
        {
            IsPlayButtonChecked = _player.IsPlaying;
            ToggleRecordingCommand.NotifyCanExecuteChanged();
        };

        Core.CoreOptions.Instance.PropertyChanged += (_, e) =>
        {
            if (nameof(PlayRepeatCount).Equals(e.PropertyName, StringComparison.Ordinal)) OnPropertyChanged(e);
        };

        _player.IsPlayingChanged += (_, _) =>
        {
            dispatcher.Enqueue(_onIsPlayingChanged);
        };
        _recorder.IsRecordingChanged += (_, _) =>
        {
            PlayActionsCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanAddAction));
        };
        _actionCollection.ActionsCountChanged += (_, _) => PlayActionsCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanToggleRecording))]
    private void ToggleRecording()
    {
        if (_recorder.IsRecording)
        {
            _recorder.StopRecording();
            return;
        }

        if (!_recorder.IsSubscribed) _recorder.RegisterRawInput();

        _recorder.StartRecording();
    }
    private bool CanToggleRecording() => !_player.IsPlaying;

    [RelayCommand(CanExecute = nameof(CanPlayActions))]
    private void PlayActions()
    {
        if (!_player.TryPlayActions())
        {
            _player.RefreshIsPlaying();
        }
    }
    private bool CanPlayActions() => !_recorder.IsRecording && (_actionCollection.Actions.Count > 0 || _actionCollection.CursorPathStart is not null);

    [RelayCommand]
    private void ToggleCursorPathWindow()
    {
        if (_pathWindowService.IsPathWindowOpen)
        {
            _pathWindowService.ClosePathWindow();
            return;
        }

        _pathWindowService.OpenPathWindow();
    }
}
