using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ActionRepeater.UI.Pages;

public sealed partial class CmdBarOptionsPage : Page
{
    private CmdBarOptionsViewModel _viewModel = null!;

    public CmdBarOptionsPage()
    {
        NavigationCacheMode = NavigationCacheMode.Required;
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _viewModel = (CmdBarOptionsViewModel)e.Parameter;
        base.OnNavigatedTo(e);
    }
}
