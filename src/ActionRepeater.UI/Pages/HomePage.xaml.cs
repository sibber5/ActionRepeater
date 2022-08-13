using ActionRepeater.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ActionRepeater.UI.Pages;

public sealed partial class HomePage : Page
{
    private readonly HomePageViewModel _viewModel;

    public HomePage()
    {
        NavigationCacheMode = NavigationCacheMode.Required;

        _viewModel = App.Current.Services.GetRequiredService<HomePageViewModel>();

        InitializeComponent();

        _repeatActionsNumBox.NumberFormatter = Helpers.NumberFormatterHelper.RoundToOneFormatter;

        Core.Input.Recorder.IsMouseOverExl ??= () => _recordButton.IsPointerOver;
    }
}
