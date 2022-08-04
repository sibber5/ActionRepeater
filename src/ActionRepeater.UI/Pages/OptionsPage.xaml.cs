using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ActionRepeater.UI.Pages;

public sealed partial class OptionsPage : Page
{
    public OptionsPageViewModel ViewModel { get; set; } = null!;

    public OptionsPage()
    {
        NavigationCacheMode = NavigationCacheMode.Required;
        InitializeComponent();

        _clickIntervalNumbox.NumberFormatter = Helpers.NumberFormatterHelper.RoundToOneFormatter;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel = (OptionsPageViewModel)e.Parameter;
        base.OnNavigatedTo(e);
    }
}
