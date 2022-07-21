using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Helpers;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Services;
using ActionRepeater.UI.Helpers;

namespace ActionRepeater.UI.ViewModels;

public partial class CmdBarHomeViewModel : ObservableObject
{
    public int PlayRepeatCount
    {
        get => Core.Options.Instance.PlayRepeatCount;
        set => Core.Options.Instance.PlayRepeatCount = value;
    }

    [ObservableProperty]
    private bool _isPlayButtonChecked;

    private readonly Func<string, string?, Task> _showContentDialog;

    private readonly PathWindowService _pathWindowService;

    private readonly ActionListViewModel _actionListVM;

    // These select the item in the list view that is currently being performed
    private readonly Action<bool> _updateActionsView;
    private readonly Action<bool> _updateActionsExlView;

    public CmdBarHomeViewModel(Func<string, string?, Task> showContentDialog, PathWindowService pathWindowService, ActionListViewModel actionListVM)
    {
        _showContentDialog = showContentDialog;
        _pathWindowService = pathWindowService;
        _actionListVM = actionListVM;

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

        Player.IsPlayingChanged += Player_IsPlayingChanged;
        Recorder.IsRecordingChanged += Recorder_IsRecordingChanged;
        ActionManager.ActionCollectionChanged += ActionManager_ActionCollectionChanged;
    }

    private void Player_IsPlayingChanged(object? sender, bool e)
    {
        IsPlayButtonChecked = e;
        ToggleRecordingCommand.NotifyCanExecuteChanged();
    }

    private void Recorder_IsRecordingChanged(object? sender, bool e)
    {
        PlayActionsCommand.NotifyCanExecuteChanged();
    }

    private void ActionManager_ActionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add
            || e.Action == NotifyCollectionChangedAction.Remove
            || e.Action == NotifyCollectionChangedAction.Reset)
        {
            PlayActionsCommand.NotifyCanExecuteChanged();
            ExportActionsCommand.NotifyCanExecuteChanged();
        }
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
        _actionListVM.CurrentSetActionIndex = -1;
        Player.UpdateView = Core.Options.Instance.SendKeyAutoRepeat ? _updateActionsView : _updateActionsExlView;

        if (!ActionManager.TryPlayActions())
        {
            Player.RefreshIsPlaying();
            return;
        }
    }
    private static bool CanPlayActions() => !Recorder.IsRecording && ActionManager.Actions.Count > 0;

    [RelayCommand(CanExecute = nameof(CanExportActions))]
    private async Task ExportActions()
    {
        if (ActionManager.Actions.Count == 0)
        {
            await _showContentDialog("Actions list is empty.", null);
            return;
        }

        var file = await FilePickerHelper.PickSaveFileAsync();
        if (file is null) return;

        ActionData dat = new()
        {
            Actions = ActionManager.Actions,
            CursorPathStartAbs = ActionManager.CursorPathStart,
            CursorPathRel = ActionManager.CursorPath
        };

        await SerializationHelper.SerializeActionsAsync(dat, file.Path);
    }
    private static bool CanExportActions() => ActionManager.Actions.Count > 0;

    [RelayCommand]
    private async Task ImportActions()
    {
        var file = await FilePickerHelper.PickSingleFileAsync();
        if (file is null) return;

        ActionData? data = null;
        try
        {
            data = await SerializationHelper.DeserializeActionsAsync(file.Path);
        }
        catch (Exception ex)
        {
            await _showContentDialog($"❌ {file.Name} couldn't be loaded", ex.Message);
            return;
        }

        if (data is null || (data.Actions.IsNullOrEmpty() && data.CursorPathRel is null))
        {
            await _showContentDialog("Actions empty", $"{nameof(ActionData)} is null");
            return;
        }

        ActionManager.LoadActionData(data);
    }

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
