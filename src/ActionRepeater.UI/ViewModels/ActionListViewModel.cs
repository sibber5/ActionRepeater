using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Utilities;
using Microsoft.UI.Dispatching;
using ActionRepeater.UI.Services;

namespace ActionRepeater.UI.ViewModels;

public partial class ActionListViewModel : ObservableObject
{
    [NotifyPropertyChangedFor(nameof(FilteredActions))]
    [ObservableProperty]
    private bool _showKeyRepeatActions;

    internal SyncedObservableCollection<ActionViewModel, InputAction> ActionVMs { get; }
    internal SyncedObservableCollection<ActionViewModel, InputAction> ActionsExlVMs { get; }
    public IReadOnlyList<ActionViewModel> FilteredActions => ShowKeyRepeatActions ? ActionVMs : ActionsExlVMs;

    public InputAction? SelectedAction => SelectedActionIndex == -1
        ? null
        : (ShowKeyRepeatActions ? ActionManager.Actions[SelectedActionIndex] : ActionManager.ActionsExlKeyRepeat[SelectedActionIndex]);

    [NotifyCanExecuteChangedFor(nameof(StoreActionCommand))]
    [ObservableProperty]
    private int _selectedActionIndex = -1;

    public int CurrentSetActionIndex { get; set; }

    private InputAction? _copiedAction;
    public InputAction? CopiedAction
    {
        get => _copiedAction;
        set
        {
            if (_copiedAction == value) return;

            _copiedAction = value;
            AddActionCommand.NotifyCanExecuteChanged();
            ReplaceActionCommand.NotifyCanExecuteChanged();
        }
    }

    public bool CanAddAction => !Recorder.IsRecording;

    internal Action? ScrollToSelectedItem { get; set; }

    private readonly ContentDialogService _contentDialogService;

    private readonly DispatcherQueueHandler _updateSelectedAction;

    public ActionListViewModel(ContentDialogService contentDialogService)
    {
        _contentDialogService = contentDialogService;

        _updateSelectedAction = () =>
        {
            SelectedActionIndex = CurrentSetActionIndex;
            ScrollToSelectedItem?.Invoke();
        };

        Func<InputAction?, ActionViewModel> createVM = static (model) => new ActionViewModel(model!);

        ActionVMs = new((ObservableCollection<InputAction?>)ActionManager.Actions, createVM);
        ActionsExlVMs = new((ObservableCollection<InputAction?>)ActionManager.ActionsExlKeyRepeat, createVM);

        Recorder.IsRecordingChanged += (_, _) => OnPropertyChanged(nameof(CanAddAction));
    }

    public void UpdateActionListIndex(bool skipAutoRepeat)
    {
        CurrentSetActionIndex++;

        if (skipAutoRepeat)
        {
            var filteredActions = ShowKeyRepeatActions ? ActionManager.Actions : ActionManager.ActionsExlKeyRepeat;
            for (int i = CurrentSetActionIndex; i < filteredActions.Count; i++)
            {
                if (IsAutoRepeatAction(filteredActions[i]) || (i > 0 && filteredActions[i] is WaitAction && IsAutoRepeatAction(filteredActions[i - 1])))
                {
                    continue;
                }

                CurrentSetActionIndex = i;
                break;
            }

            static bool IsAutoRepeatAction(InputAction a) => a is KeyAction keyAction && keyAction.IsAutoRepeat;
        }

        if (CurrentSetActionIndex >= FilteredActions.Count) CurrentSetActionIndex = -1;

        App.MainWindow.DispatcherQueue.TryEnqueue(_updateSelectedAction);
    }

    [RelayCommand]
    internal async Task EditSelectedAction()
    {
        if (SelectedAction is null) throw new InvalidOperationException($"{nameof(SelectedAction)} is null");

        ObservableObject editActionVM = SelectedAction switch
        {
            KeyAction ka => new EditKeyActionViewModel(ka),
            MouseButtonAction mba => new EditMouseButtonActionViewModel(mba),
            MouseWheelAction mwa => new EditMouseWheelActionViewModel(mwa),
            WaitAction wa => new EditWaitActionViewModel(wa),
            _ => throw new NotSupportedException($"{SelectedAction.GetType()} not suppored.")
        };

        await _contentDialogService.ShowEditActionDialog(editActionVM, SelectedAction);
    }

    private bool IsSelectedActionNotAutoRepeat() => SelectedAction is not KeyAction ka || !ka.IsAutoRepeat;

    [RelayCommand(CanExecute = nameof(IsSelectedActionNotAutoRepeat))]
    private void StoreAction() => CopiedAction = SelectedAction;

    private bool IsActionStored() => CopiedAction is not null;

    [RelayCommand(CanExecute = nameof(IsActionStored))]
    private void AddAction() => ActionManager.AddAction(CopiedAction!.Clone());

    [RelayCommand(CanExecute = nameof(IsActionStored))]
    private async Task ReplaceAction()
    {
        string? message = ActionManager.TryReplaceAction(!ShowKeyRepeatActions, SelectedActionIndex, CopiedAction!.Clone());
        if (message is not null)
        {
            await _contentDialogService.ShowErrorDialog("Failed to replace action", message);
        }
    }

    [RelayCommand]
    private async Task RemoveAction()
    {
        if (ActionManager.HasActionBeenModified(SelectedAction!))
        {
            await _contentDialogService.ShowYesNoMessageDialog("Are you sure you want to remove this action?",
                $"This action represents multiple hidden actions (because \"{nameof(ShowKeyRepeatActions)}\" is off).{Environment.NewLine}If you remove it the multiple actions it represents will be removed.",
                onYesClick: () => ActionManager.TryRemoveAction(SelectedAction!));

            return;
        }

        string? message = ActionManager.TryRemoveAction(SelectedAction!);
        if (message is not null)
        {
            await _contentDialogService.ShowErrorDialog("Failed to remove action", message);
        }
    }
}
