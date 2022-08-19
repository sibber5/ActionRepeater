using ActionRepeater.Core.Extentions;
using ActionRepeater.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ActionRepeater.UI.Views;

public sealed partial class ActionListView : UserControl
{
    private readonly ActionListViewModel _viewModel;

    public ActionListView()
    {
        _viewModel = App.Current.Services.GetRequiredService<ActionListViewModel>();
        _viewModel.ScrollToSelectedItem = ScrollToSelectedItem;

        InitializeComponent();
    }

    public void ScrollToSelectedItem() => ActionList.ScrollIntoView(ActionList.SelectedItem);

    private void ActionList_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is ActionViewModel actionItem)
        {
            ActionList.SelectedIndex = _viewModel.FilteredActions.RefIndexOfReverse(actionItem);
            ActionItemMenuFlyout.ShowAt(ActionList, e.GetPosition(ActionList));
            return;
        }

        ActionList.SelectedIndex = -1;
        ActionListMenuFlyout.ShowAt(ActionList, e.GetPosition(ActionList));
    }

    private async void ActionList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is not ActionViewModel actionVM)
        {
            ActionList.SelectedIndex = -1;
            return;
        }

        if (!ReferenceEquals(ActionList.SelectedItem, actionVM)) ActionList.SelectedItem = actionVM;

        await _viewModel.EditSelectedAction();
    }
}
