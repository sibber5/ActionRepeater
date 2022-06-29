using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Input;

namespace ActionRepeater.UI.ViewModels;

public partial class ActionListViewModel : ObservableObject
{
    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(FilteredActions))]
    private bool _showKeyRepeatActions;

    public IReadOnlyList<InputAction> FilteredActions { get => ShowKeyRepeatActions ? ActionManager.Actions : ActionManager.ActionsExlKeyRepeat; }

    [ObservableProperty]
    private InputAction? _selectedAction;

    [ObservableProperty]
    private int _selectedActionIndex = -1;

    public IRelayCommand CopyCommand { get; }
    public IRelayCommand PasteCommand { get; }
    public IRelayCommand ReplaceCommand { get; }
    public IAsyncRelayCommand RemoveCommand { get; }
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
            PasteCommand.NotifyCanExecuteChanged();
            ReplaceCommand.NotifyCanExecuteChanged();
        }
    }

    private readonly Func<string, string?, Task> _showContentDialog;

    public ActionListViewModel(Func<string, string?, Task> showContentDialog)
    {
        _showContentDialog = showContentDialog;

        Func<bool> isCopiedActionNull = () => CopiedAction is not null;

        CopyCommand = new RelayCommand(StoreAction);
        PasteCommand = new RelayCommand(AddAction, isCopiedActionNull);
        ReplaceCommand = new RelayCommand(ReplaceAction, isCopiedActionNull);
        RemoveCommand = new AsyncRelayCommand(RemoveAction);
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

    private void StoreAction() => CopiedAction = SelectedAction;
    private void AddAction() => ActionManager.AddAction(CopiedAction!.Clone());
    private void ReplaceAction() => ActionManager.ReplaceAction(!ShowKeyRepeatActions, SelectedActionIndex, CopiedAction!.Clone());
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
