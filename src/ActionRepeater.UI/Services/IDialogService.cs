using System;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;
using ActionRepeater.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.Services;

public interface IDialogService
{
    public Task ShowErrorDialog(string title, string message);

    public Task ShowMessageDialog(string title, string? message = null);

    public Task<YesNoDialogResult> ShowYesNoDialog(string title, string? message = null, Action? onYesClick = null, Action? onNoClick = null);

    public Task<ConfirmCancelDialogResult> ShowEditActionDialog(ActionType actionType);

    public Task<ConfirmCancelDialogResult> ShowEditActionDialog(ObservableObject editActionViewModel, InputAction actionToEdit);
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
