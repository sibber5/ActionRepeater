using ActionRepeater.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
            PlayButton.IsEnabled = false;
            Player.Cancel();
            PlayButton.IsEnabled = true;
            return;
        }

        Player.PlayActions(actions, App.MainWindow.SendKeyAutoRepeat);
    }
}
