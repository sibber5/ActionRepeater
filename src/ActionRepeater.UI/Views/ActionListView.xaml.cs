using System;
using System.Diagnostics;
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
    private ActionListViewModel? _vm;
    
    public ActionListView() { }

    public void Initialize(ActionListViewModel vm, Recorder recorder, AddActionMenuItems addActionMenuItems)
    {
        if (_vm is not null) throw new UnreachableException($"{nameof(ActionListView)} has already been initialized.");

        _vm = vm;
        _vm._scrollToSelectedItem = ScrollToSelectedItem;

        recorder.ActionAdded += (_, _) => ActionList.ScrollIntoView(_vm.FilteredActions[^1]);

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

            _vm.SelectedRanges = ActionList.SelectedRanges;
        }
    }

    public void ScrollToSelectedItem() => ActionList.ScrollIntoView(ActionList.SelectedItem);

    private void ActionList_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is ActionViewModel actionItem)
        {
            int rightClickedItemIdx = _vm!.FilteredActions.RefIndexOfReverse(actionItem);
            if (!ActionList.SelectedRanges.Any(x => rightClickedItemIdx >= x.FirstIndex && rightClickedItemIdx <= x.LastIndex))
            {
                ActionList.SelectedIndex = rightClickedItemIdx;
                SingleItemMenuFlyout.ShowAt(ActionList, e.GetPosition(ActionList));
                return;
            }

            if (AreMultipleItemsSelected())
            {
                MultiItemMenuFlyout.ShowAt(ActionList, e.GetPosition(ActionList));
                return;
            }

            SingleItemMenuFlyout.ShowAt(ActionList, e.GetPosition(ActionList));
            return;
        }

        ActionList.SelectedIndex = -1;
        NoItemMenuFlyout.ShowAt(ActionList, e.GetPosition(ActionList));
    }

    private async void ActionList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is not ActionViewModel actionVM) return;

        if (!ReferenceEquals(ActionList.SelectedItem, actionVM)) ActionList.SelectedItem = actionVM;

        await _vm!.EditSelectedAction();
    }

    private void ActionList_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is ActionViewModel)
        {
            _vm!.SelectedRanges = ActionList.SelectedRanges;
            return;
        }

        ActionList.SelectedIndex = -1;
    }

    private bool AreMultipleItemsSelected() => ActionList.SelectedRanges.Count > 1 || (ActionList.SelectedRanges.Count == 1 && ActionList.SelectedRanges[0].FirstIndex != ActionList.SelectedRanges[0].LastIndex);
}
