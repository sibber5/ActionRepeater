using System;
using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ActionRepeater.UI.Pages;

public sealed partial class CmdBarHomePage : Page
{
    public CmdBarHomeViewModel ViewModel { get; set; } = null!;

    public CmdBarHomePage()
    {
        NavigationCacheMode = NavigationCacheMode.Required;
        InitializeComponent();

        _repeatActionsNumBox.NumberFormatter = Helpers.NumberFormatterHelper.RoundToOneFormatter;

        Core.Input.Recorder.IsMouseOverExl ??= () => _recordButton.IsPointerOver;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel = (CmdBarHomeViewModel)e.Parameter;
        base.OnNavigatedTo(e);
    }
}
