using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views;

public sealed partial class OptionsView : UserControl
{
    private readonly OptionsViewModel _vm;

    public OptionsView(OptionsViewModel vm)
    {
        _vm = vm;

        InitializeComponent();

        _clickIntervalNumbox.NumberFormatter = Helpers.NumberFormatterHelper.RoundToOneFormatter;
    }

    private void MouseAccelerationWarning_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _mouseAccelerationWarningPopup.IsOpen = true;
    }
}
