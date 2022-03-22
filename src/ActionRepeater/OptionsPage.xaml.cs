using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater;

public sealed partial class OptionsPage : Page
{
    private void CursorMovementModeChanged(object s, SelectionChangedEventArgs e)
    {
        Options.Instance.CursorMovementMode = (CursorMovementMode)CursorMovementModeCB.SelectedIndex;
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

        CursorMovementModeCB.SelectedIndex = (int)Options.Instance.CursorMovementMode;
        ClickIntervalNumBox.Value = Options.Instance.MaxClickInterval;
    }
}
