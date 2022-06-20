using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ActionRepeater.Core.Helpers;
using ActionRepeater.Core.Input;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ActionRepeater.UI.Commands;

public class ExportActionsCommand : CommandBase
{
    private readonly Func<string, string?, Task> _showContentDialog;

    public ExportActionsCommand(Func<string, string?, Task> showContentDialog)
    {
        _showContentDialog = showContentDialog;

        ActionManager.ActionCollectionChanged += Actions_CollectionChanged;
    }

    private void Actions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add
            || e.Action == NotifyCollectionChangedAction.Remove
            || e.Action == NotifyCollectionChangedAction.Reset)
        {
            RaiseCanExecuteChanged();
        }
    }

    public override bool CanExecute(object? parameter) => ActionManager.Actions.Count > 0;

    public override async void Execute(object? parameter)
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
}
