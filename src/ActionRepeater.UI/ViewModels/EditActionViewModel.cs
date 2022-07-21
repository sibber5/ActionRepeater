using ActionRepeater.Core.Action;
using ActionRepeater.Core.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.ViewModels;

public partial class EditActionViewModel : ObservableObject
{
    public bool CanChangeAction { get; }

    [ObservableProperty]
    private int _selectedActionIndex;
    partial void OnSelectedActionIndexChanged(int value) => CurrentEditActionViewModel = GetViewModelForAction((SelectedAction)value);

    public SelectedAction SelectedAction
    {
        get => (SelectedAction)SelectedActionIndex;
        set => SelectedActionIndex = (int)value;
    }

    [ObservableProperty]
    private ObservableObject? _currentEditActionViewModel;
    partial void OnCurrentEditActionViewModelChanging(ObservableObject? value)
    {
        if (_currentEditActionViewModel is ObservableValidator currentValidator)
        {
            currentValidator.ErrorsChanged -= Validator_ErrorsChanged;
        }

        if (value is ObservableValidator validator)
        {
            validator.ErrorsChanged += Validator_ErrorsChanged;
        }
    }

    // reference to content dialog to manually enable and disabled add button, because CanExecute doesnt do that for some reason.
    private readonly ContentDialog _contentDialog;

    public EditActionViewModel(SelectedAction action, ContentDialog contentDialog, bool canChangeAction = true)
    {
        CanChangeAction = canChangeAction;
        SelectedAction = action;
        CurrentEditActionViewModel = GetViewModelForAction(action);
        _contentDialog = contentDialog;
    }

    public EditActionViewModel(ObservableObject actionViewModel, ContentDialog contentDialog, bool canChangeAction = false)
    {
        CanChangeAction = canChangeAction;
        SelectedAction = GetActionForViewModel(actionViewModel);
        CurrentEditActionViewModel = actionViewModel;
        _contentDialog = contentDialog;
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

        ActionManager.AddAction(action, addAutoRepeatIfKeyUpAct: true);
    }

    private static ObservableObject? GetViewModelForAction(SelectedAction action) => action switch
    {
        SelectedAction.KeyAction => new EditKeyActionViewModel(),
        SelectedAction.MouseButtonAction => new EditMouseButtonActionViewModel(),
        SelectedAction.MouseWheelAction => new EditMouseWheelActionViewModel(),
        SelectedAction.WaitAction => new EditWaitActionViewModel(),
        _ => null,
    };

    private static SelectedAction GetActionForViewModel(ObservableObject view) => view switch
    {
        EditKeyActionViewModel => SelectedAction.KeyAction,
        EditMouseButtonActionViewModel => SelectedAction.MouseButtonAction,
        EditMouseWheelActionViewModel => SelectedAction.MouseWheelAction,
        EditWaitActionViewModel => SelectedAction.WaitAction,
        _ => (SelectedAction)(-1)
    };
}

public enum SelectedAction
{
    KeyAction,
    MouseButtonAction,
    MouseWheelAction,
    WaitAction
}
