using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views;

public sealed partial class EditWaitActionView : UserControl
{
    // can't make it a required prop because xaml source gen doesnt support that.
    public EditWaitActionViewModel ViewModel { get; set; } = null!;

    public EditWaitActionView()
    {
        InitializeComponent();
    }
}
