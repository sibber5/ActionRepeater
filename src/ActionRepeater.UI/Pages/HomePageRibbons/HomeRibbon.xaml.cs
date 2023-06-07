using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ActionRepeater.UI.Pages.HomePageRibbons;

public sealed partial class HomeRibbon : Page
{
    private HomePageViewModel? _vm;

    public HomeRibbon()
    {
        NavigationCacheMode = NavigationCacheMode.Required;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (_vm is null)
        {
            var (vm, _, recorder, _) = (HomePageParameter)e.Parameter;

            _vm = vm;

            InitializeComponent();

            _repeatActionsNumBox.NumberFormatter = Helpers.NumberFormatterHelper.RoundToOneFormatter;

            recorder.ShouldRecordMouseClick ??= () => _recordButton.IsPointerOver;
        }

        base.OnNavigatedTo(e);
    }
}
