using System;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Factories;
using ActionRepeater.UI.Helpers;
using ActionRepeater.UI.ViewModels;
using ActionRepeater.UI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Services;

public sealed class ContentDialogService : IDialogService
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

    public async Task ShowOkDialog(string title, object? content = null)
    {
        await new ContentDialog()
        {
            XamlRoot = _windowProperties.XamlRoot,
            Title = title,
            Content = content,
            CloseButtonText = "Ok",
        }.ShowAsync();
    }

    public async Task ShowErrorDialog(string title, string message)
    {
        await new ContentDialog()
        {
            XamlRoot = _windowProperties.XamlRoot,
            Title = $"❌ {title}",
            Content = message,
            CloseButtonText = "Ok",
        }.ShowAsync();
    }

    public async Task<YesNoDialogResult> ShowYesNoDialog(string title, string? message = null, Action? onYesClick = null, Action? onNoClick = null)
    {
        var result = await new ContentDialog()
        {
            XamlRoot = _windowProperties.XamlRoot,
            Title = title,
            Content = message,
            PrimaryButtonText = "Yes",
            PrimaryButtonCommand = onYesClick is null ? null : new RelayCommand(onYesClick),
            SecondaryButtonText = "No",
            SecondaryButtonCommand = onNoClick is null ? null : new RelayCommand(onNoClick),
        }.ShowAsync();

        return result switch
        {
            ContentDialogResult.None => YesNoDialogResult.None,
            ContentDialogResult.Primary => YesNoDialogResult.Yes,
            ContentDialogResult.Secondary => YesNoDialogResult.No,
            _ => throw new NotImplementedException()
        };
    }

    public async Task<ConfirmCancelDialogResult> ShowEditActionDialog(ActionType actionType)
    {
        ContentDialog dialog = new()
        {
            XamlRoot = _windowProperties.XamlRoot,
            PrimaryButtonText = "Add",
            SecondaryButtonText = "Cancel",
        };

        EditActionViewModel vm = _editActionViewModelFactory.Create(actionType, dialog);

        dialog.Content = new EditActionView(isAddView: true) { ViewModel = vm };
        dialog.PrimaryButtonCommand = vm.AddActionCommand;

        var result = await dialog.ShowAsync();

        return result switch
        {
            ContentDialogResult.None => ConfirmCancelDialogResult.None,
            ContentDialogResult.Primary => ConfirmCancelDialogResult.Confirm,
            ContentDialogResult.Secondary => ConfirmCancelDialogResult.Cancel,
            _ => throw new NotImplementedException()
        };
    }

    public async Task<ConfirmCancelDialogResult> ShowEditActionDialog(ObservableObject editActionViewModel, InputAction actionToEdit)
    {
        if (_actionCollection.IsActionTiedToAggregateAction(actionToEdit))
        {
            await ShowErrorDialog("This action is not editable.", (actionToEdit is KeyAction { IsAutoRepeat: true })
                ? "This is an auto repeat action, edit the original key down action if you want to change the key."
                : ActionCollection.ActionTiedToAggregateActionMsg);

            return ConfirmCancelDialogResult.None;
        }

        ContentDialog dialog = new()
        {
            XamlRoot = _windowProperties.XamlRoot,
            PrimaryButtonText = "Update",
            SecondaryButtonText = "Cancel",
        };

        EditActionViewModel vm = _editActionViewModelFactory.Create(editActionViewModel, dialog);

        dialog.Content = new EditActionView() { ViewModel = vm };
        dialog.PrimaryButtonCommand = vm.UpdateActionCommand;
        dialog.PrimaryButtonCommandParameter = actionToEdit;

        var result = await dialog.ShowAsync();

        return result switch
        {
            ContentDialogResult.None => ConfirmCancelDialogResult.None,
            ContentDialogResult.Primary => ConfirmCancelDialogResult.Confirm,
            ContentDialogResult.Secondary => ConfirmCancelDialogResult.Cancel,
            _ => throw new NotImplementedException()
        };
    }
}
