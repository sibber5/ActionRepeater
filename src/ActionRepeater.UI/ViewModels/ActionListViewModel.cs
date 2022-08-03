using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredActions))]
    private bool _showKeyRepeatActions;

    internal SyncedObservableCollection<ActionViewModel, InputAction> ActionVMs { get; }
    internal SyncedObservableCollection<ActionViewModel, InputAction> ActionsExlVMs { get; }
    public IReadOnlyList<ActionViewModel> FilteredActions => ShowKeyRepeatActions ? ActionVMs : ActionsExlVMs;

    public InputAction? SelectedAction => SelectedActionIndex == -1
        ? null
        : (ShowKeyRepeatActions ? ActionManager.Actions[SelectedActionIndex] : ActionManager.ActionsExlKeyRepeat[SelectedActionIndex]);

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

    public Action? ScrollToSelectedItem { get; set; }

    internal readonly ContentDialogService _contentDialogService;

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

    [RelayCommand]
    private void StoreAction() => CopiedAction = SelectedAction;

    private bool IsCopiedActionNull() => CopiedAction is not null;

    [RelayCommand(CanExecute = nameof(IsCopiedActionNull))]
    private void AddAction() => ActionManager.AddAction(CopiedAction!.Clone());

    [RelayCommand(CanExecute = nameof(IsCopiedActionNull))]
    private void ReplaceAction() => ActionManager.ReplaceAction(!ShowKeyRepeatActions, SelectedActionIndex, CopiedAction!.Clone());

    [RelayCommand]
    private async Task RemoveAction()
    {
        if (!ActionManager.TryRemoveAction(SelectedAction!))
        {
            await _contentDialogService.ShowErrorDialog(
                "Failed to remove action",
                $"This action represents multiple hidden actions (because \"{nameof(ShowKeyRepeatActions)}\" is off).{Environment.NewLine}Removing it will result in unexpected behavior.");
        }
    }
}
