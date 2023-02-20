using ActionRepeater.Core.Input;
using ActionRepeater.UI.ViewModels;
using ActionRepeater.UI.Views;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ActionRepeater.UI.Pages;

public sealed partial class HomePage : Page
{
    private HomePageViewModel? _vm;

    public HomePage()
    {
        NavigationCacheMode = NavigationCacheMode.Required;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (_vm is null)
        {
            (HomePageViewModel vm, AddActionMenuItems addActionMenuItems, Recorder recorder, ActionListViewModel actionListVM) = (HomePageParameter)e.Parameter;

            _vm = vm;

            InitializeComponent();
            _actionList.Initialize(actionListVM, recorder, addActionMenuItems);

            addActionMenuItems.AddTo(_actionsMenuFlyout.Items);

            _repeatActionsNumBox.NumberFormatter = Helpers.NumberFormatterHelper.RoundToOneFormatter;

            recorder.ShouldRecordMouseClick ??= () => _recordButton.IsPointerOver;
        }

        base.OnNavigatedTo(e);
    }
}
