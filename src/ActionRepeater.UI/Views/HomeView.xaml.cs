using System;
using System.Diagnostics;
using ActionRepeater.UI.Views.HomeViewRibbons;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views;

public sealed partial class HomeView : UserControl
{
    private readonly ActionListView _actionListView;

    private readonly HomeRibbon _homeRibbon;
    private readonly AddRibbon _addRibbon;

    public HomeView(ActionListView actionListView, HomeRibbon homeRibbon, AddRibbon addRibbon)
    {
        _actionListView = actionListView;
        _homeRibbon = homeRibbon;
        _addRibbon = addRibbon;

        InitializeComponent();
    }

    public void NavigateRibbon(string? tag, bool isNavigateRight, bool suppressTransition = false)
    {
        Debug.Assert(tag?.StartsWith("h_", StringComparison.Ordinal) == true);

        object newView = tag switch
        {
            MainWindow.HomeRibbonTag => _homeRibbon,
            MainWindow.AddRibbonTag => _addRibbon,
            _ => throw new NotImplementedException()
        };

        _ribbonPresenter.Navigate(newView, isNavigateRight, suppressTransition);
    }
}
