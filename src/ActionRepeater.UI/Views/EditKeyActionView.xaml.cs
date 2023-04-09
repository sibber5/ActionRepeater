using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views;

public sealed partial class EditKeyActionView : UserControl
{
    // can't make it a required prop because xaml source gen doesnt support that.
    public EditKeyActionViewModel ViewModel { get; set; } = null!;

    public EditKeyActionView()
    {
        InitializeComponent();
    }

    private void TextBox_GotFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ((TextBox)sender).SelectAll();
    }
}
