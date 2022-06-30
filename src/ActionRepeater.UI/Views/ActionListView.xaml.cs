using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ActionRepeater.UI.ViewModels;

namespace ActionRepeater.UI.Views;

public sealed partial class ActionListView : UserControl
{
    public ActionListViewModel ViewModel { get; set; } = null!;

    public ActionListView()
    {
        InitializeComponent();
    }

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
