using System;
using System.Diagnostics;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.ViewModels;

public sealed partial class EditActionViewModel : ObservableObject
{
    public bool CanChangeAction { get; }

    [ObservableProperty]
    private int _selectedIndex = -1;
    partial void OnSelectedIndexChanged(int value) => CurrentEditActionViewModel = ActionMappingHelper.ActionTypeToEditViewModel(SelectedActionType);

    public ActionType SelectedActionType
    {
        get => SelectedIndex switch
        {
            0 => ActionType.KeyAction,
            1 => ActionType.MouseButtonAction,
            2 => ActionType.MouseWheelAction,
            3 => ActionType.WaitAction,
            5 => ActionType.TextTypeAction,
            _ => throw new UnreachableException()
        };

        set => SelectedIndex = value switch
        {
            ActionType.KeyAction => 0,
            ActionType.MouseButtonAction => 1,
            ActionType.MouseWheelAction => 2,
            ActionType.WaitAction => 3,
            ActionType.TextTypeAction => 5,
            _ => throw new UnreachableException()
        };
    }

    // _currentEditActionViewModel *is* set in both constructors, but static analysis doesnt detect that.
    // It's set through its property, or in OnSelectedActionTypeChanged, when SelectedActionType is set.
    [ObservableProperty]
    private ObservableObject _currentEditActionViewModel = null!;
    partial void OnCurrentEditActionViewModelChanging(ObservableObject value)
    {
        if (_currentEditActionViewModel is ObservableValidator oldValidator)
        {
            oldValidator.ErrorsChanged -= Validator_ErrorsChanged;
        }

        if (value is ObservableValidator newValidator)
        {
            newValidator.ErrorsChanged += Validator_ErrorsChanged;
        }
    }

    // reference to content dialog to manually enable and disabled add button, because CanExecute doesnt do that for some reason.
    private readonly ContentDialog _contentDialog;

    private readonly ActionCollection _actionsCollection;

    public EditActionViewModel(ActionType actionType, ContentDialog contentDialog, ActionCollection actionsCollection, bool canChangeAction = true)
    {
        CanChangeAction = canChangeAction;

        SelectedActionType = actionType;

        _contentDialog = contentDialog;
        _actionsCollection = actionsCollection;
    }

    public EditActionViewModel(ObservableObject editActionViewModel, ContentDialog contentDialog, ActionCollection actionsCollection, bool canChangeAction = false)
    {
        CanChangeAction = canChangeAction;

        CurrentEditActionViewModel = editActionViewModel;
        _selectedIndex = (int)ActionMappingHelper.EditViewModelToActionType(editActionViewModel);

        _contentDialog = contentDialog;
        _actionsCollection = actionsCollection;
    }

    private void Validator_ErrorsChanged(object? sender, System.ComponentModel.DataErrorsChangedEventArgs e)
    {
        _contentDialog.IsPrimaryButtonEnabled = !((ObservableValidator)sender!).HasErrors;
    }

    // CanExecute doesnt disable ContentDialog's button for some reason, so its done manually in ErrorsChanged's event handler
    [RelayCommand]
    private void AddAction()
    {
        InputAction action = ActionMappingHelper.EditViewModelToAction(CurrentEditActionViewModel);

        _actionsCollection.Add(action, addAutoRepeatIfActIsKeyUp: true);
    }

    [RelayCommand]
    private void UpdateAction(InputAction action)
    {
        switch (CurrentEditActionViewModel)
        {
            case EditKeyActionViewModel keyVM:
                var ka = (KeyAction)action;

                if (ka.ActionType == KeyActionType.KeyDown)
                {
                    _actionsCollection.TryReplace(false, _actionsCollection.ActionsAsSpan.RefIndexOfReverse(action), new KeyAction(keyVM.Type, keyVM.Key));
                    break;
                }

                ka.ActionType = keyVM.Type;
                ka.Key = keyVM.Key;
                break;

            case EditMouseButtonActionViewModel mbVM:
                var mba = (MouseButtonAction)action;
                mba.ActionType = mbVM.Type;
                mba.Button = mbVM.Button;
                mba.Position = mbVM.Position;
                break;

            case EditMouseWheelActionViewModel mwVM:
                var mwa = (MouseWheelAction)action;
                mwa.IsHorizontal = mwVM.HorizontalScrolling;
                mwa.StepCount = mwVM.Steps;
                mwa.DurationMS = (int)(mwVM.DurationSecs * 1000);
                break;

            case EditWaitActionViewModel waitVM:
                if (_actionsCollection.IsAggregateAction(action))
                {
                    _actionsCollection.TryReplace(true, _actionsCollection.FilteredActionsAsSpan.RefIndexOfReverse(action), new WaitAction((int)(waitVM.DurationSecs * 1000)));
                    break;
                }

                var wa = (WaitAction)action;
                wa.DurationMS = (int)(waitVM.DurationSecs * 1000);
                break;

            case EditTextTypeActionViewModel textVM:
                var tta = (TextTypeAction)action;
                tta.Text = textVM.Text;
                tta.WPM = textVM.Wpm;
                break;

            default:
                throw new NotImplementedException();
        }
    }
}
