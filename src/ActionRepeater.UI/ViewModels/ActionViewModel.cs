using ActionRepeater.Core.Action;
using ActionRepeater.Core.Extentions;
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

        inputAction.NameChanged += InputAction_NameChanged;
        inputAction.DescriptionChanged += InputAction_DescriptionChanged;
    }

    // use static method to not capture `this` in order to decrease GC pressure because changing the description doesnt happen often.
    // also avoids potential memory leaks, in case the action for this view model lives longer than the vm.
    private static void InputAction_NameChanged(object? sender, string newName)
    {
        GetActionViewModel((InputAction)sender!).Name = newName;
    }

    private static void InputAction_DescriptionChanged(object? sender, string newDescription)
    {
        GetActionViewModel((InputAction)sender!).Description = newDescription;
    }

    /// <returns>The <see cref="ActionViewModel"/> for the <paramref name="action"/>, that is currently bound to in the view.</returns>
    private static ActionViewModel GetActionViewModel(InputAction action)
    {
        var actionListVM = App.MainWindow.ViewModel.ActionListViewModel;

        int index = Core.Input.ActionManager.ActionsExlKeyRepeat.RefIndexOfReverse(action);

        if (index == -1)
        {
            index = Core.Input.ActionManager.Actions.RefIndexOfReverse(action);
            return actionListVM.ActionVMs[index];
        }

        return actionListVM.ActionsExlVMs[index];
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
