using System;
using System.Threading.Tasks;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Helpers;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Helpers;
using ActionRepeater.UI.Services;
using CommunityToolkit.Mvvm.Input;

namespace ActionRepeater.UI.ViewModels;

public partial class MainViewModel
{
    private readonly ContentDialogService _contentDialogService;
    private readonly ActionCollection _actionCollection;

    public MainViewModel(ContentDialogService contentDialogService, ActionCollection actionCollection)
    {
        _contentDialogService = contentDialogService;
        _actionCollection = actionCollection;

        _actionCollection.ActionsCountChanged += (_, _) => ExportActionsCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanExportActions))]
    private async Task ExportActions()
    {
        if (_actionCollection.Actions.Count == 0)
        {
            await _contentDialogService.ShowMessageDialog("Actions list is empty.");
            return;
        }

        var file = await FilePickerHelper.PickSaveFileAsync();
        if (file is null) return;

        ActionData dat = new(_actionCollection.Actions, _actionCollection.CursorPathStart, _actionCollection.CursorPath);

        await SerializationHelper.SerializeActionsAsync(dat, file.Path);
    }
    private bool CanExportActions() => _actionCollection.Actions.Count > 0;

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
            await _contentDialogService.ShowErrorDialog($"{file.Name} couldn't be loaded", ex.Message);
            return;
        }

        if (data is null || (data.Actions.IsNullOrEmpty() && data.CursorPathRelative is null))
        {
            await _contentDialogService.ShowErrorDialog("Actions empty", $"{nameof(ActionData)} is null");
            return;
        }

        _actionCollection.LoadActionData(data);
    }
}
