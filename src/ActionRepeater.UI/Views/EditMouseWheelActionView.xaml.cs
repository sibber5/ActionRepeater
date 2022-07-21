using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views;

public sealed partial class EditMouseWheelActionView : UserControl
{
    public EditMouseWheelActionViewModel ViewModel { get; set; } = null!;

    public EditMouseWheelActionView()
    {
        InitializeComponent();

        _stepsNumerBox.NumberFormatter = Helpers.NumberFormatterHelper.RoundToOneFormatter;
    }
}
