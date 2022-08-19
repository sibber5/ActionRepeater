using System;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace ActionRepeater.UI.Services;

public class ContentDialogService
{
    // would have set it in the ctor but it must be set after MainWindow.Content has loaded.
    internal XamlRoot XamlRoot { get; set; } = null!;

    public ContentDialogService()
    {
        System.Diagnostics.Debug.WriteLineIf(XamlRoot is null, "XamlRoot should be set.");
    }

    public IAsyncOperation<ContentDialogResult> ShowErrorDialog(string title, string message)
    {
        return new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Title = $"❌ {title}",
            Content = message,
            CloseButtonText = "Ok",
        }.ShowAsync();
    }

    public IAsyncOperation<ContentDialogResult> ShowMessageDialog(string title, string? message = null)
    {
        return new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Title = title,
            Content = message,
            CloseButtonText = "Ok",
        }.ShowAsync();
    }

    public IAsyncOperation<ContentDialogResult> ShowYesNoMessageDialog(string title, string? message = null, Action? onYesClick = null, Action? onNoClick = null)
    {
        return new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Title = title,
            Content = message,
            PrimaryButtonText = "Yes",
            PrimaryButtonCommand = onYesClick is null ? null : new RelayCommand(onYesClick),
            SecondaryButtonText = "No",
            SecondaryButtonCommand = onNoClick is null ? null : new RelayCommand(onNoClick),
        }.ShowAsync();
    }

    public IAsyncOperation<ContentDialogResult> ShowEditActionDialog(ActionType actionType)
    {
        ContentDialog dialog = new()
        {
            XamlRoot = XamlRoot,
            PrimaryButtonText = "Add",
            SecondaryButtonText = "Cancel",
        };

        EditActionDialogViewModel vm = new(actionType, dialog);

        dialog.Content = new Views.EditActionView(isAddView: true) { ViewModel = vm };
        dialog.PrimaryButtonCommand = vm.AddActionCommand;

        return dialog.ShowAsync();
    }

    public IAsyncOperation<ContentDialogResult> ShowEditActionDialog(ObservableObject editActionViewModel, InputAction actionToEdit)
    {
        if (ActionManager.IsActionTiedToModifiedAction(actionToEdit))
        {
            return ShowErrorDialog("This action is not editable.", (actionToEdit is KeyAction ka && ka.IsAutoRepeat)
                ? "This is an auto repeat action, edit the original key down action if you want to change the key."
                : ActionManager.ActionTiedToModifiedActMsg);
        }

        ContentDialog dialog = new()
        {
            XamlRoot = XamlRoot,
            PrimaryButtonText = "Update",
            SecondaryButtonText = "Cancel",
        };

        EditActionDialogViewModel vm = new(editActionViewModel, dialog);

        dialog.Content = new Views.EditActionView() { ViewModel = vm };
        dialog.PrimaryButtonCommand = vm.UpdateActionCommand;
        dialog.PrimaryButtonCommandParameter = actionToEdit;

        return dialog.ShowAsync();
    }
}
