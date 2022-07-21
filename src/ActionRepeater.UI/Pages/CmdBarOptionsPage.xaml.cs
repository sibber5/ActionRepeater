using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ActionRepeater.UI.Pages;

public sealed partial class CmdBarOptionsPage : Page
{
    public CmdBarOptionsViewModel ViewModel { get; set; } = null!;

    public CmdBarOptionsPage()
    {
        NavigationCacheMode = NavigationCacheMode.Required;
        InitializeComponent();

        _clickIntervalNumbox.NumberFormatter = Helpers.NumberFormatterHelper.RoundToOneFormatter;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel = (CmdBarOptionsViewModel)e.Parameter;
        base.OnNavigatedTo(e);
    }
}
