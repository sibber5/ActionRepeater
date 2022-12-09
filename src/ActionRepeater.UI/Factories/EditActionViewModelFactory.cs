using ActionRepeater.Core.Input;
using ActionRepeater.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Factories;

public sealed class EditActionViewModelFactory
{
    private readonly ActionCollection _actionCollection;

    public EditActionViewModelFactory(ActionCollection actionCollection)
    {
        _actionCollection = actionCollection;
    }

    public EditActionViewModel Create(ActionType actionType, ContentDialog contentDialog, bool canChangeAction = true)
        => new(actionType, contentDialog, _actionCollection, canChangeAction);

    public EditActionViewModel Create(ObservableObject editActionViewModel, ContentDialog contentDialog, bool canChangeAction = false)
        => new(editActionViewModel, contentDialog, _actionCollection, canChangeAction);
}
