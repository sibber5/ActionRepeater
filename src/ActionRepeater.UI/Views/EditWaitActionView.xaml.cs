using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views;

public sealed partial class EditWaitActionView : UserControl
{
    public EditWaitActionViewModel ViewModel { get; set; } = null!;

    public EditWaitActionView()
    {
        InitializeComponent();
    }
}
