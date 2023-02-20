using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ActionRepeater.UI.Pages;

public sealed partial class OptionsPage : Page
{
    private OptionsPageViewModel? _vm;

    public OptionsPage()
    {
        NavigationCacheMode = NavigationCacheMode.Required;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (_vm is null)
        {
            _vm = ((OptionsPageParameter)e.Parameter).VM;

            InitializeComponent();

            _clickIntervalNumbox.NumberFormatter = Helpers.NumberFormatterHelper.RoundToOneFormatter;
        }

        base.OnNavigatedTo(e);
    }

    private void MouseAccelerationWarning_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _mouseAccelerationWarningPopup.IsOpen = true;
    }
}
