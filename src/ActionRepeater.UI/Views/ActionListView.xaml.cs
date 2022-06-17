using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ActionRepeater.Core.Action;
using ActionRepeater.UI.ViewModels;
using System.Threading.Tasks;

namespace ActionRepeater.UI.Views;

public sealed partial class ActionListView : UserControl
{
    public ActionListViewModel ViewModel { get; set; } = null!;

    public ActionListView()
    {
        InitializeComponent();
    }

    public async Task ShowErrorDialog(string title, string content)
    {
        await new ContentDialog
        {
            XamlRoot = ActionList.XamlRoot,
            Title = $"❌ {title}",
            Content = content,
            CloseButtonText = "Ok"
        }.ShowAsync();
    }

    private void ActionList_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var listView = (ListView)sender;

        if (((FrameworkElement)e.OriginalSource).DataContext is InputAction actionItem)
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

            //ActionItemMenuFlyout.Items[1].IsEnabled = _copiedAction is not null; // enable replace button if copied action isnt null
            ActionItemMenuFlyout.ShowAt(listView, e.GetPosition(listView));

            return;
        }

        //ActionListMenuFlyout.Items[0].IsEnabled = _copiedAction is not null; // enable paste button if copied action isnt null
        ActionListMenuFlyout.ShowAt(listView, e.GetPosition(listView));
    }
}
