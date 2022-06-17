using System;
using ActionRepeater.UI.Pages;
using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views;

public sealed partial class CommandBarView : UserControl
{
    public CommandBarViewModel ViewModel { get; set; } = null!;

    private const string HomeTag = "h";
    private const string OptionsTag = "o";

    private string HomeTagProp => HomeTag;
    private string OptionsTagProp => OptionsTag;

    public CommandBarView()
    {
        InitializeComponent();
    }

    private void CmdBarNavView_Loaded(object sender, RoutedEventArgs e)
    {
        _cmdBarNavView.SelectedItem = _cmdBarNavView.MenuItems[0];
        NavigateCommandBar(HomeTag, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
    }

    private void CmdBarNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        NavigateCommandBar(args.SelectedItemContainer.Tag.ToString(), args.RecommendedNavigationTransitionInfo);
    }

    private void NavigateCommandBar(string? tag, Microsoft.UI.Xaml.Media.Animation.NavigationTransitionInfo navInfo)
    {
        switch (tag)
        {
            case HomeTag:
                _contentFrame.Navigate(typeof(CmdBarHomePage), ViewModel.HomeViewModel, navInfo);
                break;

            case OptionsTag:
                _contentFrame.Navigate(typeof(CmdBarOptionsPage), ViewModel.OptionsViewModel, navInfo);
                break;
        }
    }
}
