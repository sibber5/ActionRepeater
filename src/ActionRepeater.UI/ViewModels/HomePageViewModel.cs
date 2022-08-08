using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Services;

namespace ActionRepeater.UI.ViewModels;

public partial class HomePageViewModel : ObservableObject
{
    public ActionListViewModel ActionListViewModel { get; }

    public int PlayRepeatCount
    {
        get => Core.Options.Instance.PlayRepeatCount;
        set => Core.Options.Instance.PlayRepeatCount = value;
    }

    [ObservableProperty]
    private bool _isPlayButtonChecked;

    public bool CanAddAction => !Recorder.IsRecording;

    private readonly PathWindowService _pathWindowService;

    // These select the item in the list view that is currently being performed
    private readonly Action<bool> _updateActionsView;
    private readonly Action<bool> _updateActionsExlView;

    public HomePageViewModel(ActionListViewModel actionListVM, PathWindowService pathWindowService)
    {
        ActionListViewModel = actionListVM;

        _pathWindowService = pathWindowService;

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

        Core.Options.Instance.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PlayRepeatCount)) OnPropertyChanged(e);
        };

        Player.IsPlayingChanged += (_, newVal) =>
        {
            IsPlayButtonChecked = newVal;
            ToggleRecordingCommand.NotifyCanExecuteChanged();
        };
        Recorder.IsRecordingChanged += (_, _) =>
        {
            PlayActionsCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanAddAction));
        };
        ActionManager.ActionsCountChanged += (_, _) => PlayActionsCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanToggleRecording))]
    private static void ToggleRecording()
    {
        if (Recorder.IsRecording)
        {
            Recorder.StopRecording();
            return;
        }

        if (!Recorder.IsSubscribed) Recorder.RegisterRawInput(App.MainWindow.Handle);

        Recorder.StartRecording();
    }
    private static bool CanToggleRecording() => !Player.IsPlaying;

    [RelayCommand(CanExecute = nameof(CanPlayActions))]
    private void PlayActions()
    {
        ActionListViewModel.CurrentSetActionIndex = -1;
        Player.UpdateView = Core.Options.Instance.SendKeyAutoRepeat ? _updateActionsView : _updateActionsExlView;

        if (!ActionManager.TryPlayActions())
        {
            Player.RefreshIsPlaying();
            return;
        }
    }
    private static bool CanPlayActions() => !Recorder.IsRecording && ActionManager.Actions.Count > 0;

    [RelayCommand]
    private void ToggleCursorPathWindow()
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
