using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ActionRepeater.UI.Pages;

public sealed partial class HomePage : Page
{
    public HomePageViewModel ViewModel { get; set; } = null!;

    public HomePage()
    {
        NavigationCacheMode = NavigationCacheMode.Required;
        InitializeComponent();

        _repeatActionsNumBox.NumberFormatter = Helpers.NumberFormatterHelper.RoundToOneFormatter;

        Core.Input.Recorder.IsMouseOverExl ??= () => _recordButton.IsPointerOver;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel = (HomePageViewModel)e.Parameter;
        base.OnNavigatedTo(e);
    }
}
