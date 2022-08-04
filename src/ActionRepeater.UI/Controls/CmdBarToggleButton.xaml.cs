using System.Windows.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace ActionRepeater.UI.Controls;

public sealed partial class CmdBarToggleButton : UserControl
{
    public string Glyph
    {
        get => _fontIcon.Glyph;
        set => _fontIcon.Glyph = value;
    }

    public double FontIconSize
    {
        get => _fontIcon.FontSize;
        set => _fontIcon.FontSize = value;
    }

    public string Text
    {
        get => _textBlcok.Text;
        set => _textBlcok.Text = value;
    }

    public double ButtonWidth
    {
        get => _stackpanel.Width;
        set => _stackpanel.Width = value;
    }

    public double ButtonHeight
    {
        get => _stackpanel.Height;
        set => _stackpanel.Height = value;
    }

    public bool? IsChecked
    {
        get => _toggleButton.IsChecked;
        set => _toggleButton.IsChecked = value;
    }

    public ICommand Command
    {
        get => _toggleButton.Command;
        set => _toggleButton.Command = value;
    }

    public object CommandParameter
    {
        get => _toggleButton.CommandParameter;
        set => _toggleButton.CommandParameter = value;
    }

    /// <inheritdoc cref="ButtonBase.IsPointerOver"/>
    public bool IsPointerOver => _toggleButton.IsPointerOver;

    public CmdBarToggleButton()
    {
        InitializeComponent();
    }
}
