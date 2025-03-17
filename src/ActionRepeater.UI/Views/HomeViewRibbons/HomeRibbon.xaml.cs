using ActionRepeater.Core.Input;
using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views.HomeViewRibbons;

public sealed partial class HomeRibbon : UserControl
{
    private readonly HomeViewModel _vm;

    public HomeRibbon(HomeViewModel vm, Recorder recorder)
    {
        _vm = vm;
        
        InitializeComponent();

        _repeatActionsNumBox.NumberFormatter = Helpers.NumberFormatterHelper.RoundToOneFormatter;

        recorder.ShouldRecordMouseClick ??= () => _recordButton.IsPointerOver;
    }
}
