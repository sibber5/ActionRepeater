using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ActionRepeater.UI.ViewModels;

namespace ActionRepeater.UI.Views;

public sealed partial class ActionListView : UserControl
{
    private ActionListViewModel _viewModel = null!;
    public ActionListViewModel ViewModel
    {
        get => _viewModel;
        set
        {
            _viewModel = value;
            _viewModel.ScrollToSelectedItem = ScrollToSelectedItem;
        }
    }

    public ActionListView()
    {
        InitializeComponent();
    }

    public void ScrollToSelectedItem() => ActionList.ScrollIntoView(ActionList.SelectedItem);

    private void ActionList_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var listView = (ListView)sender;

        if (((FrameworkElement)e.OriginalSource).DataContext is ActionViewModel actionItem)
        {
            //ActionList.SelectedItem = actionItem;
            var col = ViewModel.FilteredActions;
            for (int i = 0; i < col.Count; ++i)
            {
                if (ReferenceEquals(col[i], actionItem))
                {
                    ActionList.SelectedIndex = i;
                    break;
                }
            }

            ActionItemMenuFlyout.ShowAt(listView, e.GetPosition(listView));

            return;
        }

        ActionListMenuFlyout.ShowAt(listView, e.GetPosition(listView));
    }
}
