using ActionRepeater.Core.Input;
using ActionRepeater.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Factories;

public class EditActionDialogViewModelFactory
{
    private readonly ActionCollection _actionCollection;

    public EditActionDialogViewModelFactory(ActionCollection actionCollection)
    {
        _actionCollection = actionCollection;
    }

    public EditActionDialogViewModel Create(ActionType actionType, ContentDialog contentDialog, bool canChangeAction = true)
        => new(actionType, contentDialog, _actionCollection, canChangeAction);

    public EditActionDialogViewModel Create(ObservableObject editActionViewModel, ContentDialog contentDialog, bool canChangeAction = false)
        => new(editActionViewModel, contentDialog, _actionCollection, canChangeAction);
}
