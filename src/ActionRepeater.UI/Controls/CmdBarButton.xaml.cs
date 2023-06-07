using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace ActionRepeater.UI.Controls;

public sealed partial class CmdBarButton : UserControl
{
    public new Thickness Padding
    {
        get => _button.Padding;
        set => _button.Padding = value;
    }

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

    public new double FontSize
    {
        get => _textBlcok.FontSize;
        set => _textBlcok.FontSize = value;
    }

    public string Text
    {
        get => _textBlcok.Text;
        set => _textBlcok.Text = value;
    }

    public double ButtonWidth
    {
        get
        {
            var val = (double)GetValue(ButtonWidthProperty);
            if (val != _stackpanel.Width) _stackpanel.Width = val;
            return val;
        }
        set
        {
            SetValue(ButtonWidthProperty, value);
            _stackpanel.Width = value;
        }
    }

    // Using a DependencyProperty as the backing store for ButtonWidth.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ButtonWidthProperty =
        DependencyProperty.Register("ButtonWidth", typeof(double), typeof(CmdBarButton), new PropertyMetadata(0));

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
