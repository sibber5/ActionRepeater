using ActionRepeater.Core.Action;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.ViewModels;

public sealed partial class ActionViewModel : ObservableObject
{
    public double GlyphSize { get; }

    public string? Glyph { get; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _description;

    private static ActionListViewModel _actionListViewModel = null!;

    private static ActionCollection _actionCollection = null!;

    public ActionViewModel(InputAction inputAction, ActionListViewModel actionListViewModel, ActionCollection actionCollection)
    {
        _actionListViewModel ??= actionListViewModel;
        _actionCollection ??= actionCollection;

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
        var (actionVM, actionExlVM) = GetActionViewModels((InputAction)sender!);
        if (actionVM is not null) actionVM.Name = newName;
        if (actionExlVM is not null) actionExlVM.Name = newName;
    }

    private static void InputAction_DescriptionChanged(object? sender, string newDescription)
    {
        var (actionVM, actionExlVM) = GetActionViewModels((InputAction)sender!);
        if (actionVM is not null) actionVM.Description = newDescription;
        if (actionExlVM is not null) actionExlVM.Description = newDescription;
    }

    /// <returns>The <see cref="ActionViewModel"/> for the <paramref name="action"/>, that is currently bound to in the view.</returns>
    private static (ActionViewModel? actionsVM, ActionViewModel? actionsExlVM) GetActionViewModels(InputAction action)
    {
        int actionsIndex = _actionCollection.ActionsAsSpan.RefIndexOfReverse(action);
        ActionViewModel? actionVM = actionsIndex == -1 ? null : _actionListViewModel.ActionVMs[actionsIndex];

        int actionsExlIndex = _actionCollection.ActionsExlKeyRepeatAsSpan.RefIndexOfReverse(action);
        ActionViewModel? actionExlVM = actionsExlIndex == -1 ? null : _actionListViewModel.ActionsExlVMs[actionsExlIndex];

        return (actionVM, actionExlVM);
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
