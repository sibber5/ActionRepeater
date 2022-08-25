using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Input;
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

        App.Current.Services.GetRequiredService<Recorder>().ActionAdded += (_, _) => ActionList.ScrollIntoView(_viewModel.FilteredActions[^1]);

        InitializeComponent();

        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.SelectedActionIndex))
        {
            if (_viewModel.SelectedActionIndex < 0)
            {
                _replaceMenuItem.KeyboardAccelerators[0].IsEnabled = false;
                _pasteMenuItem.KeyboardAccelerators[0].IsEnabled = true;
                return;
            }

            _replaceMenuItem.KeyboardAccelerators[0].IsEnabled = true;
            _pasteMenuItem.KeyboardAccelerators[0].IsEnabled = false;
        }
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
            return;
        }

        if (!ReferenceEquals(ActionList.SelectedItem, actionVM)) ActionList.SelectedItem = actionVM;

        await _viewModel.EditSelectedAction();
    }

    private void ActionList_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is ActionViewModel) return;

        ActionList.SelectedIndex = -1;
    }
}
