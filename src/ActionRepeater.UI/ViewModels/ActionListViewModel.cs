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

    public IRelayCommand ClearCommand { get; }
    public IRelayCommand ClearActionsCommand { get; }
    public IRelayCommand ClearCursorPathCommand { get; }

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

    private readonly Func<string, string?, Task> _showContentDialog;

    public ActionListViewModel(Func<string, string?, Task> showContentDialog)
    {
        _showContentDialog = showContentDialog;

        Func<InputAction?, ActionViewModel> createVM = static (model) => new ActionViewModel(model!);

        ActionVMs = new((ObservableCollection<InputAction?>)ActionManager.Actions, createVM);
        ActionsExlVMs = new((ObservableCollection<InputAction?>)ActionManager.ActionsExlKeyRepeat, createVM);

        ClearCommand = new RelayCommand(ActionManager.ClearAll, static () => ActionManager.Actions.Count > 0 || ActionManager.CursorPathStart is not null);
        ClearActionsCommand = new RelayCommand(ActionManager.ClearActions, static () => ActionManager.Actions.Count > 0);
        ClearCursorPathCommand = new RelayCommand(ActionManager.ClearCursorPath, static () => ActionManager.CursorPathStart is not null);

        ActionManager.ActionCollectionChanged += ActionManager_ActionCollectionChanged;
        ActionManager.CursorPathStartChanged += ActionManager_CursorPathStartChanged;
    }

    private void ActionManager_ActionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add
            || e.Action == NotifyCollectionChangedAction.Remove
            || e.Action == NotifyCollectionChangedAction.Reset)
        {
            ClearCommand.NotifyCanExecuteChanged();
            ClearActionsCommand.NotifyCanExecuteChanged();
        }
    }

    private void ActionManager_CursorPathStartChanged(object? sender, MouseMovement? e)
    {
        ClearCommand.NotifyCanExecuteChanged();
        ClearCursorPathCommand.NotifyCanExecuteChanged();
    }

    private bool IsCopiedActionNull() => CopiedAction is not null;

    [RelayCommand]
    private void StoreAction() => CopiedAction = SelectedAction;

    [RelayCommand(CanExecute = nameof(IsCopiedActionNull))]
    private void AddAction() => ActionManager.AddAction(CopiedAction!.Clone());

    [RelayCommand(CanExecute = nameof(IsCopiedActionNull))]
    private void ReplaceAction() => ActionManager.ReplaceAction(!ShowKeyRepeatActions, SelectedActionIndex, CopiedAction!.Clone());

    [RelayCommand]
    private async Task RemoveAction()
    {
        if (!ActionManager.TryRemoveAction(SelectedAction!))
        {
            await _showContentDialog(
                "❌ Failed to remove action",
                $"This action represents multiple hidden actions (because \"{nameof(ShowKeyRepeatActions)}\" is off).{Environment.NewLine}Removing it will result in unexpected behavior.");
        }
    }
}
