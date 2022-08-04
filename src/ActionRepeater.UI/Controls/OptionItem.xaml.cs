using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Controls;

public sealed partial class OptionItem : UserControl
{
    public string Text
    {
        get => _textBlock.Text;
        set => _textBlock.Text = value;
    }

    public object OptionContent
    {
        get => _content.Content;
        set => _content.Content = value;
    }

    public OptionItem()
    {
        this.InitializeComponent();
    }
}
