using ActionRepeater.Core.Action;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.ViewModels;

public sealed partial class EditActionDialogViewModel : ObservableObject
{
    public bool CanChangeAction { get; }

    [ObservableProperty]
    private int _selectedActionIndex;
    partial void OnSelectedActionIndexChanged(int value) => CurrentEditActionViewModel = GetViewModelForAction((ActionType)value);

    public ActionType SelectedAction
    {
        get => (ActionType)SelectedActionIndex;
        set => SelectedActionIndex = (int)value;
    }

    [ObservableProperty]
    private ObservableObject? _currentEditActionViewModel;
    partial void OnCurrentEditActionViewModelChanging(ObservableObject? value)
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

    public EditActionDialogViewModel(ActionType actionType, ContentDialog contentDialog, ActionCollection actionsCollection, bool canChangeAction = true)
    {
        CanChangeAction = canChangeAction;
        SelectedAction = actionType;
        CurrentEditActionViewModel = GetViewModelForAction(actionType);

        _contentDialog = contentDialog;

        _actionsCollection = actionsCollection;
    }

    public EditActionDialogViewModel(ObservableObject editActionViewModel, ContentDialog contentDialog, ActionCollection actionsCollection, bool canChangeAction = false)
    {
        CanChangeAction = canChangeAction;
        SelectedAction = GetActionForViewModel(editActionViewModel);
        CurrentEditActionViewModel = editActionViewModel;

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
        InputAction action = CurrentEditActionViewModel switch
        {
            EditKeyActionViewModel keyVM => new KeyAction(keyVM.Type, keyVM.Key),
            EditMouseButtonActionViewModel mbVM => new MouseButtonAction(mbVM.Type, mbVM.Button, mbVM.Position),
            EditMouseWheelActionViewModel mwVM => new MouseWheelAction(mwVM.HorizontalScrolling, mwVM.Steps, (int)(mwVM.DurationSecs * 1000)),
            EditWaitActionViewModel waitVM => new WaitAction((int)(waitVM.DurationSecs * 1000)),
            _ => throw new System.NotSupportedException($"{CurrentEditActionViewModel?.GetType()} not supported.")
        };

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
                if (_actionsCollection.HasActionBeenModified(action))
                {
                    _actionsCollection.TryReplace(true, _actionsCollection.ActionsExlKeyRepeatAsSpan.RefIndexOfReverse(action), new WaitAction((int)(waitVM.DurationSecs * 1000)));
                    break;
                }

                var wa = (WaitAction)action;
                wa.DurationMS = (int)(waitVM.DurationSecs * 1000);
                break;

            default:
                throw new System.NotSupportedException($"{CurrentEditActionViewModel?.GetType()} not supported.");
        }
    }

    private static ObservableObject? GetViewModelForAction(ActionType actionType) => actionType switch
    {
        ActionType.KeyAction => new EditKeyActionViewModel(),
        ActionType.MouseButtonAction => new EditMouseButtonActionViewModel(),
        ActionType.MouseWheelAction => new EditMouseWheelActionViewModel(),
        ActionType.WaitAction => new EditWaitActionViewModel(),
        _ => throw new System.ArgumentException($"{actionType} not supported", nameof(actionType))
    };

    private static ActionType GetActionForViewModel(ObservableObject editActionViewModel) => editActionViewModel switch
    {
        EditKeyActionViewModel => ActionType.KeyAction,
        EditMouseButtonActionViewModel => ActionType.MouseButtonAction,
        EditMouseWheelActionViewModel => ActionType.MouseWheelAction,
        EditWaitActionViewModel => ActionType.WaitAction,
        _ => throw new System.ArgumentException("Action VM Not supported", nameof(editActionViewModel))
    };
}

public enum ActionType
{
    KeyAction,
    MouseButtonAction,
    MouseWheelAction,
    WaitAction
}
