using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ActionRepeater.UI.Services;

public sealed class FilePicker : IFilePicker
{
    private readonly WindowProperties _windowProperties;

    public FilePicker(WindowProperties windowProperties)
    {
        _windowProperties = windowProperties;
    }

    public async Task<FileInfo?> PickSaveFileAsync(params (string typeName, string[] typeExtensions)[] fileTypeChoices)
    {
        FileSavePicker savePicker = new()
        {
            SuggestedFileName = "Actions"
        };

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, _windowProperties.Handle);

        foreach (var (typeName, typeExtensions) in fileTypeChoices)
        {
            savePicker.FileTypeChoices.Add(typeName, typeExtensions);
        }

        StorageFile? file = await savePicker.PickSaveFileAsync();
        return file is null ? null : new(file.Name, file.Path);
    }

    public async Task<FileInfo?> PickSingleFileAsync(params string[] fileTypeFilter)
    {
        FileOpenPicker openPicker = new();

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, _windowProperties.Handle);

        foreach (string extension in fileTypeFilter)
        {
            openPicker.FileTypeFilter.Add(extension);
        }

        StorageFile? file = await openPicker.PickSingleFileAsync();
        return file is null ? null : new(file.Name, file.Path);
    }
}
