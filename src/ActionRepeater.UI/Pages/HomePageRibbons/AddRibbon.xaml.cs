using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ActionRepeater.UI.Pages.HomePageRibbons;

public sealed partial class AddRibbon : Page
{
    private HomePageViewModel? _vm;

    public AddRibbon()
    {
        NavigationCacheMode = NavigationCacheMode.Required;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (_vm is null)
        {
            (HomePageViewModel vm, _, _, _) = (HomePageParameter)e.Parameter;

            _vm = vm;

            InitializeComponent();
        }

        base.OnNavigatedTo(e);
    }
}
