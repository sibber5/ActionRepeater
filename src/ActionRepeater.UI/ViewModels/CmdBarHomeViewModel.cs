using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Helpers;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Services;

namespace ActionRepeater.UI.ViewModels;

public class CmdBarHomeViewModel : ObservableObject
{
    private static readonly PropertyChangedEventArgs _playButtonCheckedChangedArgs = new(nameof(IsPlayButtonChecked));

    public bool IsPlayButtonChecked => Player.IsPlaying;

    public IRelayCommand RecordCommand { get; }
    public IRelayCommand PlayCommand { get; }
    public IAsyncRelayCommand ExportCommand { get; }
    public IAsyncRelayCommand ImportCommand { get; }
    public IRelayCommand ShowCursorPathCommand { get; }

    private readonly Func<string, string?, Task> _showContentDialog;

    private readonly PathWindowService _pathWindowService;

    public CmdBarHomeViewModel(Func<string, string?, Task> showContentDialog, PathWindowService pathWindowService)
    {
        _showContentDialog = showContentDialog;
        _pathWindowService = pathWindowService;

        RecordCommand = new RelayCommand(ToggleRecordingActions, static () => !Player.IsPlaying);
        PlayCommand = new RelayCommand(PlayActions, () => !Recorder.IsRecording && ActionManager.Actions.Count > 0);
        ExportCommand = new AsyncRelayCommand(ExportActions, () => ActionManager.Actions.Count > 0);
        ImportCommand = new AsyncRelayCommand(ImportActions);
        ShowCursorPathCommand = new RelayCommand(ToggleCursorPathWindow);

        Player.IsPlayingChanged += Player_IsPlayingChanged;
        Recorder.IsRecordingChanged += Recorder_IsRecordingChanged;
        ActionManager.ActionCollectionChanged += ActionManager_ActionCollectionChanged;
    }

    private void Player_IsPlayingChanged(object? sender, bool e)
    {
        OnPropertyChanged(nameof(_playButtonCheckedChangedArgs));
        RecordCommand.NotifyCanExecuteChanged();
    }

    private void Recorder_IsRecordingChanged(object? sender, bool e)
    {
        PlayCommand.NotifyCanExecuteChanged();
    }

    private void ActionManager_ActionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add
            || e.Action == NotifyCollectionChangedAction.Remove
            || e.Action == NotifyCollectionChangedAction.Reset)
        {
            PlayCommand.NotifyCanExecuteChanged();
            ExportCommand.NotifyCanExecuteChanged();
        }
    }

    private static void ToggleRecordingActions()
    {
        if (Recorder.IsRecording)
        {
            Recorder.StopRecording();
            return;
        }

        if (!Recorder.IsSubscribed) Recorder.RegisterRawInput(App.MainWindow.Handle);

        Recorder.StartRecording();
    }

    private static void PlayActions()
    {
        if (!ActionManager.TryPlayActions())
        {
            Player.RefreshIsPlaying();
        }
    }

    private async Task ExportActions()
    {
        if (ActionManager.Actions.Count == 0)
        {
            await _showContentDialog("Actions list is empty.", null);
            return;
        }

        FileSavePicker savePicker = new()
        {
            SuggestedFileName = "Actions"
        };

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, App.MainWindow.Handle);

        savePicker.FileTypeChoices.Add("ActionRepeater Actions", new List<string>() { ".acts" });
        savePicker.FileTypeChoices.Add("JSON", new List<string>() { ".json" });

        StorageFile file = await savePicker.PickSaveFileAsync();
        if (file is null) return;

        ActionData dat = new()
        {
            Actions = ActionManager.Actions,
            CursorPathStartAbs = ActionManager.CursorPathStart,
            CursorPathRel = ActionManager.CursorPath
        };

        await SerializationHelper.SerializeActionsAsync(dat, file.Path);
    }

    private async Task ImportActions()
    {
        FileOpenPicker openPicker = new();

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, App.MainWindow.Handle);

        openPicker.FileTypeFilter.Add(".acts");
        //openPicker.FileTypeFilter.Add(".json");

        StorageFile file = await openPicker.PickSingleFileAsync();
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
