using ActionRepeater.Core.Action;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.ViewModels;

public partial class ActionViewModel : ObservableObject
{
    public double GlyphSize { get; }

    public string? Glyph { get; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _description;

    public ActionViewModel(string? glyph, string name, string description, double glyphSize = 20)
    {
        Glyph = glyph;
        _name = name;
        _description = description;
        GlyphSize = glyphSize;
    }

    public ActionViewModel(InputAction inputAction)
    {
        var (glyph, size) = GetIconForAction(inputAction);
        Glyph = glyph;
        _name = inputAction.Name;
        _description = inputAction.Description;
        GlyphSize = size;

        inputAction.NameChanged += (_, newName) => Name = newName;
        inputAction.DescriptionChanged += (_, newDescription) => Description = newDescription;
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
