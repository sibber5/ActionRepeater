using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace ActionRepeater.UI.Controls;

[ContentProperty(Name = nameof(Content))]
public sealed partial class OptionItem : UserControl
{
    public string Text
    {
        get => _textBlock.Text;
        set => _textBlock.Text = value;
    }

    public new object Content
    {
        get => _content.Content;
        set => _content.Content = value;
    }

    public OptionItem()
    {
        this.InitializeComponent();
    }
}
