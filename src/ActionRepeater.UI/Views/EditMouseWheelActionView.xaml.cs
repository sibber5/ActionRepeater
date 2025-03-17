using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views;

public sealed partial class EditMouseWheelActionView : UserControl
{
    // can't make it a required prop because xaml source gen doesnt support that.
    public EditMouseWheelActionViewModel ViewModel { get; set; } = null!;

    public EditMouseWheelActionView()
    {
        InitializeComponent();

        _stepsNumerBox.NumberFormatter = Helpers.NumberFormatterHelper.RoundToOneFormatter;
    }
}
