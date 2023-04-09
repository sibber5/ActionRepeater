using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Input;
using ActionRepeater.Core.Utilities;
using ActionRepeater.UI.Helpers;
using ActionRepeater.UI.Services;
using ActionRepeater.UI.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActionRepeater.UI.ViewModels;

public sealed partial class ActionListViewModel : ObservableObject
{
    private readonly PropertyChangedEventArgs _actionListHeaderChangedArgs = new(nameof(ActionListHeaderWithCount));
    public string ActionListHeaderWithCount => $"Actions ({ViewedActions.Count}):";

    [NotifyPropertyChangedFor(nameof(ViewedActions))]
    [NotifyPropertyChangedFor(nameof(CanReorderActions))]
    [ObservableProperty]
    private bool _showKeyRepeatActions;

    public SyncedObservableCollection<ActionViewModel, InputAction> ActionsVMs { get; }

    public SyncedObservableCollection<ActionViewModel, InputAction> FilteredActionsVMs { get; }

    public IReadOnlyList<ActionViewModel> ViewedActions => ShowKeyRepeatActions ? ActionsVMs : FilteredActionsVMs;

    public InputAction? SelectedAction => SelectedActionIndex == -1
                ? null
                : (ShowKeyRepeatActions ? _actionCollection.Actions[SelectedActionIndex] : _actionCollection.FilteredActions[SelectedActionIndex]);

    [NotifyCanExecuteChangedFor(nameof(StoreActionCommand))]
    [ObservableProperty]
    private int _selectedActionIndex = -1;

    [NotifyCanExecuteChangedFor(nameof(StoreMultipleActionsCommand))]
    [ObservableProperty]
    private IReadOnlyList<Microsoft.UI.Xaml.Data.ItemIndexRange>? _selectedRanges;

    public bool CanReorderActions => !ShowKeyRepeatActions;

    private ObservableCollectionEx<InputAction> _copiedActions = new();

    public bool CanAddAction => !_recorder.IsRecording;

    private readonly IDialogService _dialogService;

    private readonly Action _updateSelectedAction;

    private readonly ActionCollection _actionCollection;
    private readonly Recorder _recorder;

    private readonly IDispatcher _dispatcher;

    private readonly ManualResetEventSlim _updateSelectedActionMre = new(true);
    private bool _isSelectedActModifiedAct;
    private int _selectedActionIndexToSet;

    internal Action _scrollToSelectedItem = null!;

    public ActionListViewModel(IDialogService dialogService, ActionCollection actionCollection, Recorder recorder, IDispatcher dispatcher)
    {
        _dialogService = dialogService;
        _actionCollection = actionCollection;
        _recorder = recorder;
        _dispatcher = dispatcher;

        _updateSelectedAction = () =>
        {
            SelectedActionIndex = _selectedActionIndexToSet;
            _scrollToSelectedItem();
            _updateSelectedActionMre.Set();
        };

        Func<InputAction?, ActionViewModel> createVM = (model) => new ActionViewModel(model!, this, _actionCollection);

        ActionsVMs = new((ObservableCollection<InputAction?>)_actionCollection.Actions, createVM);
        FilteredActionsVMs = new((ObservableCollection<InputAction?>)_actionCollection.FilteredActions, createVM);

        ((INotifyPropertyChanged)ViewedActions).PropertyChanged += (_, e) =>
        {
            if (nameof(ViewedActions.Count).Equals(e.PropertyName, StringComparison.Ordinal))
            {
                OnPropertyChanged(_actionListHeaderChangedArgs);
            }
        };
        _recorder.IsRecordingChanged += (_, _) => OnPropertyChanged(nameof(CanAddAction));

        ((INotifyPropertyChanged)_copiedActions).PropertyChanged += (_, e) =>
        {
            if (nameof(_copiedActions.Count).Equals(e.PropertyName, StringComparison.Ordinal))
            {
                AddStoredActionsCommand.NotifyCanExecuteChanged();
                ReplaceActionCommand.NotifyCanExecuteChanged();
            }
        };
    }

    public void UpdateSelectedAction(InputAction? currentAction, int index)
    {
        _updateSelectedActionMre.Wait();

        if (currentAction is null)
        {
            SetSelectedActionIndex(-1);
            return;
        }

        if (!ShowKeyRepeatActions) index = _actionCollection.FilteredActionsAsSpan.RefIndexOfReverse(currentAction);

        if (index != -1)
        {
            _isSelectedActModifiedAct = false;
            SetSelectedActionIndex(index);
            return;
        }

        Debug.Assert(!ShowKeyRepeatActions);

        if (_isSelectedActModifiedAct) return;

        _isSelectedActModifiedAct = true;
        SetSelectedActionIndex(SelectedActionIndex + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetSelectedActionIndex(int i)
        {
            _selectedActionIndexToSet = i;
            _updateSelectedActionMre.Reset();
            _dispatcher.Enqueue(_updateSelectedAction);
        }
    }

    // internal because its called from code behind (on item double tap). (there is not command for double tap, only an event)
    [RelayCommand]
    internal async Task EditSelectedAction()
    {
        if (SelectedAction is null) throw new InvalidOperationException($"{nameof(SelectedAction)} is null");

        ObservableObject editActionVM = ActionMappingHelper.ActionToEditViewModel(SelectedAction);
        await _dialogService.ShowEditActionDialog(editActionVM, SelectedAction);
    }

    private bool IsSelectedActionNotAutoRepeat() => SelectedAction is not KeyAction ka || !ka.IsAutoRepeat;
    [RelayCommand(CanExecute = nameof(IsSelectedActionNotAutoRepeat))]
    private void StoreAction()
    {
        _copiedActions.Clear();
        Debug.Assert(SelectedAction is not null);
        _copiedActions.Add(SelectedAction!);
    }

    private bool CanStoreMultipleActions()
    {
        if (SelectedRanges is null) return false;

        if (SelectedRanges.Count != 1) return false;

        if (SelectedRanges[0].FirstIndex == SelectedRanges[0].LastIndex) return false;

        if (!ShowKeyRepeatActions) return true;

        var range = SelectedRanges[0];
        for (int i = range.FirstIndex; i < range.LastIndex + 1; i++)
        {
            if (_actionCollection.FilteredActionsAsSpan[i] is KeyAction { IsAutoRepeat: true }) return false;
        }

        return true;
    }
    [RelayCommand(CanExecute = nameof(CanStoreMultipleActions))]
    private void StoreMultipleActions()
    {
        _copiedActions.Clear();
        _copiedActions.AddRange(GetSelectedActions());
    }

    private bool IsAnyActionStored() => _copiedActions.Count > 0;
    [RelayCommand(CanExecute = nameof(IsAnyActionStored))]
    private void AddStoredActions()
    {
        foreach (InputAction action in _copiedActions)
        {
            _actionCollection.Add(action.Clone());
        }
    }

    private bool IsSingleActionStored() => _copiedActions.Count == 1;
    [RelayCommand(CanExecute = nameof(IsSingleActionStored))]
    private async Task ReplaceAction()
    {
        string? errorMsg = _actionCollection.TryReplace(!ShowKeyRepeatActions, SelectedActionIndex, _copiedActions[0].Clone());
        if (errorMsg is not null)
        {
            await _dialogService.ShowErrorDialog("Failed to replace action", errorMsg);
        }
    }

    [RelayCommand]
    private async Task RemoveAction()
    {
        if (SelectedAction is null) return;

        if (_actionCollection.IsAggregateAction(SelectedAction))
        {
            var result = await _dialogService.ShowYesNoDialog("Are you sure you want to remove this action?",
                $"This action represents multiple hidden actions (because \"{nameof(ShowKeyRepeatActions)}\" is off).{Environment.NewLine}If you remove it the multiple actions it represents will also be removed.");

            if (result == YesNoDialogResult.Yes) _actionCollection.TryRemove(SelectedAction!);

            return;
        }

        string? errorMsg = _actionCollection.TryRemove(SelectedAction);
        if (errorMsg is not null)
        {
            await _dialogService.ShowErrorDialog("Failed to remove action", errorMsg);
        }
    }

    [RelayCommand]
    private async Task RemoveMultipleActions()
    {
        Debug.Assert(SelectedRanges is not null);

        // in case this is called when there is only one action selected, e.g. when pressing the keyboard shortcut
        if (SelectedRanges.Count == 1 && (SelectedRanges[0].FirstIndex == SelectedRanges[0].LastIndex))
        {
            await RemoveAction();
            return;
        }

        Debug.Assert(SelectedRanges.Count > 1 || (SelectedRanges.Count == 1 && SelectedRanges[0].FirstIndex != SelectedRanges[0].LastIndex));

        var selectedActions = GetSelectedActions().ToArray();

        if (selectedActions.Any(_actionCollection.IsAggregateAction))
        {
            var result = await _dialogService.ShowYesNoDialog("Are you sure you want to remove these actions?",
                $"One or more of the selected actions represents multiple hidden actions (because \"{nameof(ShowKeyRepeatActions)}\" is off).{Environment.NewLine}If you remove it the multiple actions it represents will also be removed.");

            if (result != YesNoDialogResult.Yes) return;
        }

        string? errorMsg;
        for (int i = 0; i < selectedActions.Length - 1; i++)
        {
            errorMsg = _actionCollection.TryRemove(selectedActions[i], mergeWaitActions: false);
            if (errorMsg is not null)
            {
                await _dialogService.ShowErrorDialog("Failed to remove action", errorMsg);
            }
        }

        // work around to merge the actions, until ActionedCollection is refactored.
        errorMsg = _actionCollection.TryRemove(selectedActions[^1], mergeWaitActions: true);
        if (errorMsg is not null)
        {
            await _dialogService.ShowErrorDialog("Failed to remove action", errorMsg);
        }
    }

    private IEnumerable<InputAction> GetSelectedActions()
    {
        var actions = ShowKeyRepeatActions ? _actionCollection.Actions : _actionCollection.FilteredActions;
        for (int i = 0; i < SelectedRanges!.Count; i++)
        {
            var range = SelectedRanges![i];
            for (int j = range.FirstIndex; j < range.LastIndex + 1; j++)
            {
                yield return actions[j];
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanClearActions))]
    private void ClearActions() => _actionCollection.ClearActions();
    private bool CanClearActions() => _actionCollection.Actions.Count > 0;

    [RelayCommand(CanExecute = nameof(CanClearCursorPath))]
    private void ClearCursorPath() => _actionCollection.ClearCursorPath();
    private bool CanClearCursorPath() => _actionCollection.CursorPathStart is not null;

    [RelayCommand(CanExecute = nameof(CanClearAll))]
    private void ClearAll() => _actionCollection.Clear();
    private bool CanClearAll() => CanClearActions() || CanClearCursorPath();
}
