using System.ComponentModel;
using System.Windows.Input;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Controls;

public sealed partial class CmdBarToggleButton : UserControl
{
    [Description("The FontIcon Glyph"), Category("Data")]
    public string Glyph
    {
        get => _fontIcon.Glyph;
        set => _fontIcon.Glyph = value;
    }

    [Description("Button Text"), Category("Data")]
    public string Text
    {
        get => _textBlcok.Text;
        set => _textBlcok.Text = value;
    }

    [Description("Button Width"), Category("Data")]
    public double ButtonWidth
    {
        get => _stackpanel.Width;
        set => _stackpanel.Width = value;
    }

    [Description("Button Height"), Category("Data")]
    public double ButtonHeight
    {
        get => _stackpanel.Height;
        set => _stackpanel.Height = value;
    }

    [Category("Data")]
    public bool? IsChecked
    {
        get => _toggleButton.IsChecked;
        set => _toggleButton.IsChecked = value;
    }

    [Description("Button Command"), Category("Data")]
    public ICommand Command
    {
        get => _toggleButton.Command;
        set => _toggleButton.Command = value;
    }

    public CmdBarToggleButton()
    {
        InitializeComponent();
    }
}
