using System;
using System.Threading.Tasks;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Helpers;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Services;
using CommunityToolkit.Mvvm.Input;

namespace ActionRepeater.UI.ViewModels;

public sealed partial class MainViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ActionCollection _actionCollection;
    private readonly IFilePicker _filePicker;

    private readonly (string typeName, string[] typeExtensions)[] _saveFileTypes = new[]
    {
        ("ActionRepeater Actions", new[] { ".ara" }),
        ("JSON", new[] { ".json" })
    };

    private readonly string[] _loadFileTypes = new[] { ".ara" };

    public MainViewModel(IDialogService dialogService, ActionCollection actionCollection, IFilePicker filePicker)
    {
        _dialogService = dialogService;
        _actionCollection = actionCollection;
        _filePicker = filePicker;
        _actionCollection.ActionsCountChanged += (_, _) => ExportActionsCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanExportActions))]
    private async Task ExportActions()
    {
        if (_actionCollection.Actions.Count == 0)
        {
            await _dialogService.ShowMessageDialog("Actions list is empty.");
            return;
        }

        var file = await _filePicker.PickSaveFileAsync(_saveFileTypes);
        if (file is null) return;

        ActionData dat = new(_actionCollection.Actions, _actionCollection.CursorPathStart, _actionCollection.CursorPath);

        await SerializationHelper.SerializeActionsToFileAsync(dat, file.Path);
    }
    private bool CanExportActions() => _actionCollection.Actions.Count > 0;

    [RelayCommand]
    private async Task ImportActions()
    {
        var file = await _filePicker.PickSingleFileAsync(_loadFileTypes);
        if (file is null) return;

        ActionData? data = null;
        try
        {
            data = await SerializationHelper.DeserializeActionsFromFileAsync(file.Path);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorDialog($"{file.Name} couldn't be loaded", ex.Message);
            return;
        }

        if (data is null || (data.Actions.IsNullOrEmpty() && data.CursorPathRelative is null))
        {
            await _dialogService.ShowErrorDialog("Actions empty", $"{nameof(ActionData)} is null");
            return;
        }

        _actionCollection.LoadActionData(data);
    }
}
