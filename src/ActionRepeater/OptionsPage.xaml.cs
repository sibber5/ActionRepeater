using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater;

public sealed partial class OptionsPage : Page
{
    private void CursorTrackingModeChanged(object s, SelectionChangedEventArgs e) => App.MainWindow.CursorTrackingMode = (CursorTrackingMode)CursorTrackingModeCB.SelectedIndex;

    private void MaxClickIntervalChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        App.MainWindow.MaxClickInterval = (int)System.Math.Round(args.NewValue);
        sender.Value = App.MainWindow.MaxClickInterval;
    }

    public OptionsPage()
    {
        this.InitializeComponent();
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Required;

        CursorTrackingModeCB.SelectedIndex = (int)App.MainWindow.CursorTrackingMode;
        ClickIntervalNumBox.Value = App.MainWindow.MaxClickInterval;
    }
}
