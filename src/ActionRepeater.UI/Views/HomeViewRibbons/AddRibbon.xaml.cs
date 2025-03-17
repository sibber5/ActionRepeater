using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views.HomeViewRibbons;

public sealed partial class AddRibbon : UserControl
{
    private readonly HomeViewModel _vm;

    public AddRibbon(HomeViewModel vm)
    {
        _vm = vm;

        InitializeComponent();
    }
}
