using System;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Factories;
using ActionRepeater.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace ActionRepeater.UI.Services;

public sealed class ContentDialogService
{
    private readonly WindowProperties _windowProperties;
    private readonly ActionCollection _actionCollection;
    private readonly EditActionViewModelFactory _editActionViewModelFactory;

    public ContentDialogService(WindowProperties windowProperties, ActionCollection actionCollection, EditActionViewModelFactory editActionViewModelFactory)
    {
        _windowProperties = windowProperties;
        _actionCollection = actionCollection;
        _editActionViewModelFactory = editActionViewModelFactory;
    }

    public IAsyncOperation<ContentDialogResult> ShowErrorDialog(string title, string message) => new ContentDialog()
    {
        XamlRoot = _windowProperties.XamlRoot,
        Title = $"❌ {title}",
        Content = message,
        CloseButtonText = "Ok",
    }.ShowAsync();

    public IAsyncOperation<ContentDialogResult> ShowMessageDialog(string title, string? message = null) => new ContentDialog()
    {
        XamlRoot = _windowProperties.XamlRoot,
        Title = title,
        Content = message,
        CloseButtonText = "Ok",
    }.ShowAsync();

    /// <remarks>
    /// "Yes" is the primary button.<br/>
    /// "No" is the secondary button.
    /// </remarks>
    public IAsyncOperation<ContentDialogResult> ShowYesNoMessageDialog(string title, string? message = null, Action? onYesClick = null, Action? onNoClick = null) => new ContentDialog()
    {
        XamlRoot = _windowProperties.XamlRoot,
        Title = title,
        Content = message,
        PrimaryButtonText = "Yes",
        PrimaryButtonCommand = onYesClick is null ? null : new RelayCommand(onYesClick),
        SecondaryButtonText = "No",
        SecondaryButtonCommand = onNoClick is null ? null : new RelayCommand(onNoClick),
    }.ShowAsync();

    public IAsyncOperation<ContentDialogResult> ShowEditActionDialog(ActionType actionType)
    {
        ContentDialog dialog = new()
        {
            XamlRoot = _windowProperties.XamlRoot,
            PrimaryButtonText = "Add",
            SecondaryButtonText = "Cancel",
        };

        EditActionViewModel vm = _editActionViewModelFactory.Create(actionType, dialog);

        dialog.Content = new Views.EditActionView(isAddView: true) { ViewModel = vm };
        dialog.PrimaryButtonCommand = vm.AddActionCommand;

        return dialog.ShowAsync();
    }

    public IAsyncOperation<ContentDialogResult> ShowEditActionDialog(ObservableObject editActionViewModel, InputAction actionToEdit)
    {
        if (_actionCollection.IsActionTiedToAggregateAction(actionToEdit))
        {
            return ShowErrorDialog("This action is not editable.", (actionToEdit is KeyAction { IsAutoRepeat: true })
                ? "This is an auto repeat action, edit the original key down action if you want to change the key."
                : ActionCollection.ActionTiedToAggregateActionMsg);
        }

        ContentDialog dialog = new()
        {
            XamlRoot = _windowProperties.XamlRoot,
            PrimaryButtonText = "Update",
            SecondaryButtonText = "Cancel",
        };

        EditActionViewModel vm = _editActionViewModelFactory.Create(editActionViewModel, dialog);

        dialog.Content = new Views.EditActionView() { ViewModel = vm };
        dialog.PrimaryButtonCommand = vm.UpdateActionCommand;
        dialog.PrimaryButtonCommandParameter = actionToEdit;

        return dialog.ShowAsync();
    }
}
