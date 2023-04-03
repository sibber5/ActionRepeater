using System;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;
using ActionRepeater.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.Services;

public interface IDialogService
{
    Task ShowErrorDialog(string title, string message);

    Task ShowMessageDialog(string title, string? message = null);

    Task<YesNoDialogResult> ShowYesNoDialog(string title, string? message = null, Action? onYesClick = null, Action? onNoClick = null);

    Task<ConfirmCancelDialogResult> ShowEditActionDialog(ActionType actionType);

    Task<ConfirmCancelDialogResult> ShowEditActionDialog(ObservableObject editActionViewModel, InputAction actionToEdit);
}

public enum YesNoDialogResult
{
    /// <summary>No button was tapped.</summary>
    None,
    Yes,
    No
}

public enum ConfirmCancelDialogResult
{
    /// <summary>No button was tapped.</summary>
    None,
    Confirm,
    Cancel
}
