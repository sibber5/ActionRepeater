using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Controls;

public sealed partial class ComboBoxIconnedItem : UserControl
{
    public string Glyph
    {
        get => _fontIcon.Glyph;
        set => _fontIcon.Glyph = value;
    }

    public double GlyphSize
    {
        get => _fontIcon.FontSize;
        set => _fontIcon.FontSize = value;
    }

    public string Text
    {
        get => _textBlock.Text;
        set => _textBlock.Text = value;
    }

    public ComboBoxIconnedItem()
    {
        InitializeComponent();
    }
}
