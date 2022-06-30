using ActionRepeater.Core.Action;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.ViewModels;

public partial class ActionViewModel : ObservableObject
{
    public double GlyphSize { get; }

    [ObservableProperty]
    private string? _glyph;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _description;

    public ActionViewModel(string? glyph, string name, string description, double glyphSize = 20)
    {
        _glyph = glyph;
        _name = name;
        _description = description;
        GlyphSize = glyphSize;
    }

    public ActionViewModel(InputAction inputAction)
    {
        var (glyph, size) = GetIconForAction(inputAction);
        _glyph = glyph;
        _name = inputAction.Name;
        _description = inputAction.Description;
        GlyphSize = size;

        inputAction.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(Name):
                    Name = ((InputAction)s!).Name;
                    break;

                case nameof(Description):
                    Description = ((InputAction)s!).Description;
                    break;
            }
        };
    }

    public static (string? glyph, double size) GetIconForAction(InputAction a) => a switch
    {
        KeyAction => ("\uE92E", 22),
        MouseButtonAction => ("\uE962", 20),
        MouseWheelAction => ("\uE962", 20),
        WaitAction => ("\uED5A", 18),
        _ => (null, 20)
    };
}
