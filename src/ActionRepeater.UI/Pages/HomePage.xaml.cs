using System;
using System.Diagnostics;
using ActionRepeater.UI.Pages.HomePageRibbons;
using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace ActionRepeater.UI.Pages;

public sealed partial class HomePage : Page
{
    private HomePageViewModel? _vm;

    private HomePageParameter? _homePageParameter;

    public HomePage()
    {
        NavigationCacheMode = NavigationCacheMode.Required;
    }

    public void NavigateRibbon(string? tag, NavigationTransitionInfo navInfo)
    {
        Debug.Assert(tag?.StartsWith("h_", StringComparison.Ordinal) == true);

        var pageType = tag switch
        {
            MainWindow.HomeRibbonTag => typeof(HomeRibbon),
            MainWindow.AddRibbonTag => typeof(AddRibbon),
            _ => throw new NotImplementedException()
        };

        _ribbonFrame.Navigate(pageType, _homePageParameter, navInfo);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (_vm is null)
        {
            var (vm, addActionMenuItems, recorder, actionListVM) = _homePageParameter = (HomePageParameter)e.Parameter;

            _vm = vm;

            InitializeComponent();
            _actionList.Initialize(actionListVM, recorder, addActionMenuItems);

            NavigateRibbon(MainWindow.HomeRibbonTag, new SuppressNavigationTransitionInfo());
        }

        base.OnNavigatedTo(e);
    }
}
