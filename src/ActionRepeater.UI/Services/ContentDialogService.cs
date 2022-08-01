using System;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;
using ActionRepeater.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Services;

public class ContentDialogService
{
    // would have set it in the ctor but it must be set after MainWindow.Content has loaded.
    internal XamlRoot XamlRoot { get; set; } = null!;

    public ContentDialogService()
    {
        System.Diagnostics.Debug.WriteLineIf(XamlRoot is null, "XamlRoot should be set.");
    }

    public async Task ShowErrorDialog(string title, string message)
    {
        await new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Title = $"❌ {title}",
            Content = message,
            CloseButtonText = "Ok",
        }.ShowAsync();
    }

    public async Task ShowMessageDialog(string title, string? message = null)
    {
        await new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Title = title,
            Content = message,
            CloseButtonText = "Ok",
        }.ShowAsync();
    }

    public async Task ShowEditActionDialog(ActionType actionType)
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

        await dialog.ShowAsync();
    }

    public async Task ShowEditActionDialog(ObservableObject editActionViewModel, InputAction actionToEdit)
    {
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

        await dialog.ShowAsync();
    }
}
