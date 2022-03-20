using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater;

public sealed partial class OptionsPage : Page
{
    private void CursorTrackingModeChanged(object s, SelectionChangedEventArgs e)
    {
        Options.Instance.CursorTrackingMode = (CursorTrackingMode)CursorTrackingModeCB.SelectedIndex;
    }

    private void MaxClickIntervalChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        Options.Instance.MaxClickInterval = (int)System.Math.Round(args.NewValue);
        sender.Value = Options.Instance.MaxClickInterval;
    }

    public OptionsPage()
    {
        this.InitializeComponent();
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Required;

        CursorTrackingModeCB.SelectedIndex = (int)Options.Instance.CursorTrackingMode;
        ClickIntervalNumBox.Value = Options.Instance.MaxClickInterval;
    }
}
