using System;
using ActionRepeater.UI.Services;
using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views;

public sealed partial class CommandBarView : UserControl
{
    public CmdBarNavigationService NavigationService { get; set; } = null!;

    private string HomeTagProp => CmdBarNavigationService.Tag.Home.ToString();
    private string OptionsTagProp => CmdBarNavigationService.Tag.Options.ToString();

    public CommandBarView()
    {
        InitializeComponent();
    }

    private void CmdBarNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        //NavigateCommandBar(args.SelectedItemContainer.Tag.ToString(), args.RecommendedNavigationTransitionInfo);
        NavigationService.Navigate(args.SelectedItemContainer.Tag.ToString());
    }

    private void _cmdBarNavView_Loaded(object sender, RoutedEventArgs e)
    {
        NavigationService.Navigate(CmdBarNavigationService.Tag.Home);
    }

    //private void NavigateCommandBar(string? tag, Microsoft.UI.Xaml.Media.Animation.NavigationTransitionInfo navInfo)
    //{
    //    Type? pageType = tag switch
    //    {
    //        HomePageTag => typeof(HomePage),
    //        OptionsPageTag => typeof(OptionsPage),
    //        _ => null
    //    };

    //    if (pageType is not null && !Equals(_navViewFrame.CurrentSourcePageType, pageType))
    //    {
    //        _navViewFrame.Navigate(pageType, null, navInfo);
    //    }
    //}
}
