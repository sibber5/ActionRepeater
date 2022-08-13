using ActionRepeater.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ActionRepeater.UI.Pages;

public sealed partial class OptionsPage : Page
{
    private readonly OptionsPageViewModel _viewModel;

    public OptionsPage()
    {
        NavigationCacheMode = NavigationCacheMode.Required;

        _viewModel = App.Current.Services.GetRequiredService<OptionsPageViewModel>();

        InitializeComponent();

        _clickIntervalNumbox.NumberFormatter = Helpers.NumberFormatterHelper.RoundToOneFormatter;
    }
}
