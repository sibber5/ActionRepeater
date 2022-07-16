using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ActionRepeater.UI.Helpers;

public static class FilePickerHelper
{
    public static async Task<StorageFile?> PickSaveFileAsync()
    {
        FileSavePicker savePicker = new()
        {
            SuggestedFileName = "Actions"
        };

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, App.MainWindow.Handle);

        savePicker.FileTypeChoices.Add("ActionRepeater Actions", new List<string>() { ".acts" });
        savePicker.FileTypeChoices.Add("JSON", new List<string>() { ".json" });

        return await savePicker.PickSaveFileAsync();
    }

    public static async Task<StorageFile?> PickSingleFileAsync()
    {
        FileOpenPicker openPicker = new();

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, App.MainWindow.Handle);

        openPicker.FileTypeFilter.Add(".acts");
        //openPicker.FileTypeFilter.Add(".json");

        return await openPicker.PickSingleFileAsync();
    }
}
