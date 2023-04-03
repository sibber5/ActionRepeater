using System.Diagnostics;
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
        var (actionVM, filteredActionVM) = GetActionViewModels((InputAction)sender!);
        if (actionVM is not null) actionVM.Name = newName;
        if (filteredActionVM is not null) filteredActionVM.Name = newName;
    }

    private static void InputAction_DescriptionChanged(object? sender, string newDescription)
    {
        var (actionVM, filteredActionVM) = GetActionViewModels((InputAction)sender!);
        if (actionVM is not null) actionVM.Description = newDescription;
        if (filteredActionVM is not null) filteredActionVM.Description = newDescription;
    }

    /// <returns>The <see cref="ActionViewModel"/> for the <paramref name="action"/>, that is currently bound to in the view.</returns>
    private static (ActionViewModel? actionsVM, ActionViewModel? filteredActionVM) GetActionViewModels(InputAction action)
    {
        ActionViewModel? actionVM;
        int index = _actionCollection.ActionsAsSpan.RefIndexOfReverse(action);
        if (index >= 0 && index < _actionListViewModel.ActionsVMs.Count)
        {
            actionVM = _actionListViewModel.ActionsVMs[index];
        }
        else
        {
            actionVM = null;
            Debug.WriteLineIf(index != -1, $"{nameof(ActionViewModel)} not found at index {index}.");
        }

        ActionViewModel? filteredActionVM;
        index = _actionCollection.FilteredActionsAsSpan.RefIndexOfReverse(action);
        if (index >= 0 && index < _actionListViewModel.FilteredActionsVMs.Count)
        {
            filteredActionVM = _actionListViewModel.FilteredActionsVMs[index];
        }
        else
        {
            filteredActionVM = null;
            Debug.WriteLineIf(index != -1, $"{nameof(ActionViewModel)} not found at index {index}.");
        }

        return (actionVM, filteredActionVM);
    }

    public static (string? glyph, double size) GetIconForAction(InputAction a) => a switch
    {
        KeyAction => (ActionGlyphs.Keyboard, 22),
        MouseButtonAction => (ActionGlyphs.Mouse, 20),
        MouseWheelAction => (ActionGlyphs.Mouse, 20),
        WaitAction => (ActionGlyphs.Clock, 18),
        _ => (null, 20)
    };
}

public static class ActionGlyphs
{
    public const string Keyboard = "\uE92E";
    public const string Mouse = "\uE962";
    public const string Clock = "\uED5A";
}
