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
        get => Core.Options.Instance.PlayRepeatCount;
        set => Core.Options.Instance.PlayRepeatCount = value;
    }

    [ObservableProperty]
    private bool _isPlayButtonChecked;

    public bool CanAddAction => !_recorder.IsRecording;

    private readonly ActionListViewModel _actionListViewModel;

    private readonly PathWindowService _pathWindowService;

    private readonly ActionCollection _actionCollection;
    private readonly Recorder _recorder;
    private readonly Player _player;

    // These select the item in the list view that is currently being performed
    private readonly Action<bool> _updateActionsView;
    private readonly Action<bool> _updateActionsExlView;

    private readonly Microsoft.UI.Dispatching.DispatcherQueueHandler _onIsPlayingChanged;

    public HomePageViewModel(ActionListViewModel actionListVM, PathWindowService pathWindowService, ActionCollection actionCollection, Recorder recorder, Player player)
    {
        _actionListViewModel = actionListVM;

        _pathWindowService = pathWindowService;

        _actionCollection = actionCollection;
        _recorder = recorder;
        _player = player;

        _updateActionsView = (isAutoRepeat) =>
        {
            // assume Actions (including key auto repeat) are playing
            System.Diagnostics.Debug.Assert(Core.Options.Instance.SendKeyAutoRepeat);

            if (actionListVM.ShowKeyRepeatActions || !isAutoRepeat)
            {
                actionListVM.UpdateActionListIndex(false);
            }
        };

        _updateActionsExlView = (isAutoRepeat) =>
        {
            // assume Actions *excluding* key auto repeat are playing
            System.Diagnostics.Debug.Assert(!Core.Options.Instance.SendKeyAutoRepeat);

            actionListVM.UpdateActionListIndex(true);
        };

        _onIsPlayingChanged = () =>
        {
            IsPlayButtonChecked = _player.IsPlaying;
            ToggleRecordingCommand.NotifyCanExecuteChanged();
        };

        Core.Options.Instance.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PlayRepeatCount)) OnPropertyChanged(e);
        };

        _player.IsPlayingChanged += (_, _) =>
        {
            App.Current.MainWindow.DispatcherQueue.TryEnqueue(_onIsPlayingChanged);
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

        if (!_recorder.IsSubscribed) _recorder.RegisterRawInput(App.Current.MainWindow.Handle);

        _recorder.StartRecording();
    }
    private bool CanToggleRecording() => !_player.IsPlaying;

    [RelayCommand(CanExecute = nameof(CanPlayActions))]
    private void PlayActions()
    {
        _actionListViewModel.CurrentSetActionIndex = -1;
        _player.UpdateView = Core.Options.Instance.SendKeyAutoRepeat ? _updateActionsView : _updateActionsExlView;

        if (!_player.TryPlayActions())
        {
            _player.RefreshIsPlaying();
        }
    }
    private bool CanPlayActions() => !_recorder.IsRecording && _actionCollection.Actions.Count > 0;

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
