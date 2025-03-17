using System;
using System.Linq;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ActionRepeater.UI.Views;

public sealed partial class ActionListView : UserControl
{
    private readonly ActionListViewModel _vm;

    public ActionListView(ActionListViewModel vm, Recorder recorder, AddActionMenuItems addActionMenuItems)
    {
        _vm = vm;

        _vm._scrollToSelectedItem = () => _actionList.ScrollIntoView(_actionList.SelectedItem);

        recorder.ActionAdded += (_, _) => _actionList.ScrollIntoView(_vm.ViewedActions[^1]);

        InitializeComponent();

        addActionMenuItems.AddTo(_listFlyoutAdd.Items);

        _vm.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (nameof(_vm.SelectedActionIndex).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            bool isAnyItemSelected = _vm!.SelectedActionIndex >= 0;

            _replaceMenuItem.KeyboardAccelerators[0].IsEnabled = isAnyItemSelected;
            _pasteMenuItem.KeyboardAccelerators[0].IsEnabled = !isAnyItemSelected;

            _vm.SelectedRanges = _actionList.SelectedRanges;
        }
    }

    private void ActionList_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is ActionViewModel actionItem)
        {
            int rightClickedItemIdx = _vm!.ViewedActions.RefIndexOfReverse(actionItem);
            if (!_actionList.SelectedRanges.Any(x => rightClickedItemIdx >= x.FirstIndex && rightClickedItemIdx <= x.LastIndex))
            {
                _actionList.SelectedIndex = rightClickedItemIdx;
                _singleItemMenuFlyout.ShowAt(_actionList, e.GetPosition(_actionList));
                return;
            }

            if (AreMultipleItemsSelected())
            {
                _multiItemMenuFlyout.ShowAt(_actionList, e.GetPosition(_actionList));
                return;
            }

            _singleItemMenuFlyout.ShowAt(_actionList, e.GetPosition(_actionList));
            return;
        }

        _actionList.SelectedIndex = -1;
        _noItemMenuFlyout.ShowAt(_actionList, e.GetPosition(_actionList));
    }

    private async void ActionList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is not ActionViewModel actionVM) return;

        if (!ReferenceEquals(_actionList.SelectedItem, actionVM)) _actionList.SelectedItem = actionVM;

        await _vm!.EditSelectedAction();
    }

    private void ActionList_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is ActionViewModel)
        {
            _vm!.SelectedRanges = _actionList.SelectedRanges;
            return;
        }

        _actionList.SelectedIndex = -1;
    }

    private bool AreMultipleItemsSelected() => _actionList.SelectedRanges.Count > 1 || (_actionList.SelectedRanges.Count == 1 && _actionList.SelectedRanges[0].FirstIndex != _actionList.SelectedRanges[0].LastIndex);
}
