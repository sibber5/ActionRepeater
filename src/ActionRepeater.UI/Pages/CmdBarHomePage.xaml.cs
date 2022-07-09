using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ActionRepeater.UI.Pages;

public sealed partial class CmdBarHomePage : Page
{
    private CmdBarHomeViewModel _viewModel = null!;

    public CmdBarHomePage()
    {
        NavigationCacheMode = NavigationCacheMode.Required;
        InitializeComponent();

        Core.Input.Recorder.IsMouseOverExl ??= () => _recordButton.IsPointerOver;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _viewModel = (CmdBarHomeViewModel)e.Parameter;
        base.OnNavigatedTo(e);
    }
}
