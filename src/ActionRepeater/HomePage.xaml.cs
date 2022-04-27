using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using ActionRepeater.Helpers;
using ActionRepeater.Input;

namespace ActionRepeater;

public sealed partial class HomePage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public bool IsHoveringOverExcl => RecordButton.IsPointerOver;

    public bool IsRecordButtonEnabled
    {
        get => RecordButton.IsEnabled;
        set => RecordButton.IsEnabled = value;
    }

    private bool _isShowingCursorPath;
    public bool IsShowingCursorPath
    {
        get => _isShowingCursorPath;

        set
        {
            _isShowingCursorPath = value;
            NotifyPropertyChanged(nameof(IsShowingCursorPath));
        }
    }

    public HomePage()
    {
        this.InitializeComponent();
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Required;
    }

    private void RecordButton_Click(object sender, RoutedEventArgs e)
    {
        if (Recorder.IsRecording)
        {
            Recorder.StopRecording();
            //PlayButton.IsEnabled = true;
            return;
        }

        //PlayButton.IsEnabled = false;
        Recorder.StartRecording();
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        ActionManager.PlayActions();
    }

    private void CursorPathButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsShowingCursorPath)
        {
            App.ClosePathWindow();

            IsShowingCursorPath = false;
            return;
        }

        App.OpenPathWindow();
        IsShowingCursorPath = true;
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        var actions = ActionManager.Actions;
        if (actions.Count == 0)
        {
            await new ContentDialog()
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = "Actions list is empty.",
                CloseButtonText = "Ok"
            }.ShowAsync();

            return;
        }

        FileSavePicker savePicker = new()
        {
            SuggestedFileName = "Actions"
        };

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, App.MainWindow.Handle);

        savePicker.FileTypeChoices.Add("XML", new List<string>() { ".xml" });

        StorageFile file = await savePicker.PickSaveFileAsync();
        if (file is null) return;

        await System.Threading.Tasks.Task.Run(() =>
        {
            ActionData dat = new()
            {
                Actions = ActionManager.Actions,
                CursorPathStartAbs = ActionManager.CursorPathStart,
                CursorPathRel = ActionManager.CursorPath
            };
            SerializationHelper.Serialize(dat, file.Path);
        });
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        FileOpenPicker openPicker = new();

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, App.MainWindow.Handle);

        openPicker.FileTypeFilter.Add(".xml");

        StorageFile file = await openPicker.PickSingleFileAsync();
        if (file is null) return;

        string? errMsg = await SerializationHelper.DeserializeActionsAsync(file.Path);
        if (errMsg is not null)
        {
            await new ContentDialog()
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = $"{file.Name} couldn't be loaded",
                Content = errMsg!,
                CloseButtonText = "Ok"
            }.ShowAsync();

            return;
        }

        ActionManager.FillFilteredActionList();
    }
}
