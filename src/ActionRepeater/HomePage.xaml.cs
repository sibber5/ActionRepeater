using System;
using System.Collections.Generic;
using ActionRepeater.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ActionRepeater;

public sealed partial class HomePage : Page
{
    public bool IsHoveringOverExcl => RecordButton.IsPointerOver;

    public bool IsRecordButtonEnabled
    {
        get => RecordButton.IsEnabled;
        set => RecordButton.IsEnabled = value;
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
        var actions = App.MainWindow.Actions;
        if (actions.Count == 0)
        {
            return;
        }

        if (Player.IsPlaying)
        {
            Player.Cancel();
            return;
        }

        Player.PlayActions(App.MainWindow.SendKeyAutoRepeat ? actions : App.MainWindow.ActionsEclKeyRepeat);
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        var actions = App.MainWindow.Actions;
        if (actions.Count < 1)
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

        await System.Threading.Tasks.Task.Run(() => Helpers.SerializationHelper.Serialize(App.MainWindow.Actions, file.Path));
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        App.MainWindow.Actions.Clear();

        FileOpenPicker openPicker = new();

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, App.MainWindow.Handle);

        openPicker.FileTypeFilter.Add(".xml");

        StorageFile file = await openPicker.PickSingleFileAsync();
        if (file is null) return;

        try
        {
            await Helpers.SerializationHelper.DeserializeActionsAsync(App.MainWindow.Actions, file.Path);
        }
        catch (SystemException ex) when (ex is FormatException || (ex is System.Xml.XmlException && ex.InnerException is FormatException))
        {
            App.MainWindow.Actions.Clear();
            await new ContentDialog()
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = $"File {file.Name} couldn't be loaded",
                Content = ex.Message,
                CloseButtonText = "Ok"
            }.ShowAsync();

            return;
        }

        App.MainWindow.FillFilteredActionList();
    }
}
