using System.Windows.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace ActionRepeater.UI.Controls;

public sealed partial class CmdBarButton : UserControl
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

    public ICommand Command
    {
        get => _button.Command;
        set => _button.Command = value;
    }

    public object CommandParameter
    {
        get => _button.CommandParameter;
        set => _button.CommandParameter = value;
    }

    /// <inheritdoc cref="ButtonBase.IsPointerOver"/>
    public bool IsPointerOver => _button.IsPointerOver;

    public CmdBarButton()
    {
        InitializeComponent();
    }
}
