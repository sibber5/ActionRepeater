using System;
using System.Threading.Tasks;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Helpers;
using ActionRepeater.Core.Input;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ActionRepeater.UI.Commands;

public class ImportActionsCommand : CommandBase
{
    private readonly Func<string, string?, Task> _showContentDialog;

    public ImportActionsCommand(Func<string, string?, Task> showContentDialog)
    {
        _showContentDialog = showContentDialog;
    }

    public override async void Execute(object? parameter)
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
}
