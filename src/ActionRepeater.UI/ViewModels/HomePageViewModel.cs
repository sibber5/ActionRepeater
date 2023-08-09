using System;
using System.Threading.Tasks;
using ActionRepeater.Core;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Helpers;
using ActionRepeater.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActionRepeater.UI.ViewModels;

public sealed partial class HomePageViewModel : ObservableObject
{
    public int PlayRepeatCount
    {
        get => _coreOptions.PlayRepeatCount;
        set => _coreOptions.PlayRepeatCount = value;
    }

    [ObservableProperty]
    private bool _isPlayButtonChecked;

    [ObservableProperty]
    private bool _isDrawablePathWindowOpen;

    public bool CanAddAction => !_recorder.IsRecording;

    public IAsyncRelayCommand EditSelectedActionCommand => _actionListViewModel.EditSelectedActionCommand;
    public IAsyncRelayCommand RemoveOneOrMoreActionsCommand => _actionListViewModel.RemoveOneOrMoreActionsCommand;

    public IRelayCommand ClearActionsCommand => _actionListViewModel.ClearActionsCommand;
    public IRelayCommand ClearCursorPathCommand => _actionListViewModel.ClearCursorPathCommand;

    private readonly CoreOptions _coreOptions;

    private readonly PathWindowService _pathWindowService;
    private readonly DrawablePathWindowService _drawablePathWindowService;
    private readonly IDialogService _dialogService;

    private readonly ActionCollection _actionCollection;
    private readonly Recorder _recorder;
    private readonly Player _player;

    private readonly ActionListViewModel _actionListViewModel;

    private readonly Action _onIsPlayingChanged;

    public HomePageViewModel(CoreOptions coreOptions, PathWindowService pathWindowService, DrawablePathWindowService drawablePathWindowService, ActionCollection actionCollection, ActionListViewModel actionListVM, Player player, Recorder recorder, IDispatcher dispatcher, IDialogService dialogService)
    {
        _coreOptions = coreOptions;

        _pathWindowService = pathWindowService;
        _drawablePathWindowService = drawablePathWindowService;
        _dialogService = dialogService;

        _actionCollection = actionCollection;
        _player = player;
        _recorder = recorder;

        _actionListViewModel = actionListVM;

        _player.ActionPlayed += _actionListViewModel.UpdateSelectedAction;

        _onIsPlayingChanged = () =>
        {
            IsPlayButtonChecked = _player.IsPlaying;
            ToggleRecordingCommand.NotifyCanExecuteChanged();
        };

        _coreOptions.PropertyChanged += (_, e) =>
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

        _drawablePathWindowService.WindowClosed += () =>
        {
            dispatcher.Enqueue(() => IsDrawablePathWindowOpen = false);
        };
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
        if (!_player.PlayActions())
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
            _pathWindowService.CloseWindow();
            return;
        }

        _pathWindowService.OpenWindow();
    }

    [RelayCommand(CanExecute = nameof(CanAddAction))]
    private Task AddAction(ActionType actionType) => _dialogService.ShowEditActionDialog(actionType);

    [RelayCommand]
    private void OpenCursorPathDrawingWindow()
    {
        _drawablePathWindowService.OpenWindow(5_000_000);
        IsDrawablePathWindowOpen = true;
    }
}
