using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views;

public sealed partial class EditTextTypeActionView : UserControl
{
    // can't make it a required prop because xaml source gen doesnt support that.
    public EditTextTypeActionViewModel ViewModel { get; set; } = null!;

    public EditTextTypeActionView()
    {
        InitializeComponent();
    }
}
