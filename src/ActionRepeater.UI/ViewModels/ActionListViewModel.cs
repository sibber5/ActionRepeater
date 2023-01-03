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
using ActionRepeater.UI.Services;
using ActionRepeater.UI.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.ViewModels;

public sealed partial class ActionListViewModel : ObservableObject
{
    private readonly PropertyChangedEventArgs _actionListHeaderChangedArgs = new(nameof(ActionListHeaderWithCount));
    public string ActionListHeaderWithCount => $"Actions ({FilteredActions.Count}):";

    [NotifyPropertyChangedFor(nameof(FilteredActions))]
    [NotifyPropertyChangedFor(nameof(CanReorderActions))]
    [ObservableProperty]
    private bool _showKeyRepeatActions;

    private readonly SyncedObservableCollection<ActionViewModel, InputAction> _actionVMs;
    internal SyncedObservableCollection<ActionViewModel, InputAction> ActionVMs
    {
        get
        {
            // call getter of property in case action collection needed to be notified of a read.
            // (e.g. when moving an action, on the next read it regenerates the unfiltered actions,
            // because im too lazy to implement insert method - this works well enough)
            _ = _actionCollection.Actions;
            return _actionVMs;
        }
    }

    private readonly SyncedObservableCollection<ActionViewModel, InputAction> _actionsExlVMs;
    internal SyncedObservableCollection<ActionViewModel, InputAction> ActionsExlVMs
    {
        get
        {
            _ = _actionCollection.ActionsExlKeyRepeat;
            return _actionsExlVMs;
        }
    }

    public IReadOnlyList<ActionViewModel> FilteredActions => ShowKeyRepeatActions ? ActionVMs : ActionsExlVMs;

    public InputAction? SelectedAction => SelectedActionIndex == -1
                ? null
                : (ShowKeyRepeatActions ? _actionCollection.Actions[SelectedActionIndex] : _actionCollection.ActionsExlKeyRepeat[SelectedActionIndex]);

    [NotifyCanExecuteChangedFor(nameof(StoreActionCommand))]
    [ObservableProperty]
    private int _selectedActionIndex = -1;

    [NotifyCanExecuteChangedFor(nameof(StoreMultipleActionsCommand))]
    [ObservableProperty]
    private IReadOnlyList<Microsoft.UI.Xaml.Data.ItemIndexRange>? _selectedRanges;

    public bool CanReorderActions => !ShowKeyRepeatActions;

    private ObservableCollectionEx<InputAction> _copiedActions = new();

    public bool CanAddAction => !_recorder.IsRecording;

    internal Action ScrollToSelectedItem = null!;

    private readonly ContentDialogService _contentDialogService;

    private readonly DispatcherQueueHandler _updateSelectedAction;

    private readonly ActionCollection _actionCollection;
    private readonly Recorder _recorder;

    private readonly ManualResetEventSlim _updateSelectedActionMre = new(true);
    private bool _isSelectedActModifiedAct;
    private int _selectedActionIndexToSet;

    public ActionListViewModel(ContentDialogService contentDialogService, ActionCollection actionCollection, Recorder recorder)
    {
        _contentDialogService = contentDialogService;
        _actionCollection = actionCollection;
        _recorder = recorder;

        _updateSelectedAction = () =>
        {
            SelectedActionIndex = _selectedActionIndexToSet;
            ScrollToSelectedItem();
            _updateSelectedActionMre.Set();
        };

        Func<InputAction?, ActionViewModel> createVM = static (model) => new ActionViewModel(model!);

        _actionVMs = new((ObservableCollection<InputAction?>)_actionCollection.Actions, createVM);
        _actionsExlVMs = new((ObservableCollection<InputAction?>)_actionCollection.ActionsExlKeyRepeat, createVM);

        ((INotifyPropertyChanged)FilteredActions).PropertyChanged += (s, e) =>
        {
            if (nameof(FilteredActions.Count).Equals(e.PropertyName, StringComparison.Ordinal))
            {
                OnPropertyChanged(_actionListHeaderChangedArgs);
            }
        };
        _recorder.IsRecordingChanged += (_, _) => OnPropertyChanged(nameof(CanAddAction));

        ((INotifyPropertyChanged)_copiedActions).PropertyChanged += (s, e) =>
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

        if (!ShowKeyRepeatActions) index = _actionCollection.ActionsExlKeyRepeatAsSpan.RefIndexOfReverse(currentAction);

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
            App.Current.MainWindow.DispatcherQueue.TryEnqueue(_updateSelectedAction);
        }
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
            if (_actionCollection.ActionsExlKeyRepeatAsSpan[i] is KeyAction { IsAutoRepeat: true }) return false;
        }

        return true;
    }
    [RelayCommand(CanExecute = nameof(CanStoreMultipleActions))]
    private void StoreMultipleActions()
    {
        _copiedActions.Clear();
        _copiedActions.AddRange(GetSelectedActions());
    }

    private bool IsAnyActionStored() => _copiedActions.Count  > 0;
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
            await _contentDialogService.ShowErrorDialog("Failed to replace action", errorMsg);
        }
    }

    [RelayCommand]
    private async Task RemoveAction()
    {
        if (SelectedAction is null) return;

        if (_actionCollection.HasActionBeenModified(SelectedAction))
        {
            ContentDialogResult result = await _contentDialogService.ShowYesNoMessageDialog("Are you sure you want to remove this action?",
                $"This action represents multiple hidden actions (because \"{nameof(ShowKeyRepeatActions)}\" is off).{Environment.NewLine}If you remove it the multiple actions it represents will also be removed.");

            if (result == ContentDialogResult.Primary) _actionCollection.TryRemove(SelectedAction!);

            return;
        }

        string? errorMsg = _actionCollection.TryRemove(SelectedAction);
        if (errorMsg is not null)
        {
            await _contentDialogService.ShowErrorDialog("Failed to remove action", errorMsg);
        }
    }

    [RelayCommand]
    private async Task RemoveMultipleActions()
    {
        Debug.Assert(SelectedRanges is not null);
        Debug.Assert(SelectedRanges.Count > 1 || (SelectedRanges.Count == 1 && SelectedRanges[0].FirstIndex != SelectedRanges[0].LastIndex));

        var selectedActions = GetSelectedActions().ToArray();

        if (selectedActions.Any(_actionCollection.HasActionBeenModified))
        {
            ContentDialogResult result = await _contentDialogService.ShowYesNoMessageDialog("Are you sure you want to remove these actions?",
                $"One or more of the selected actions represents multiple hidden actions (because \"{nameof(ShowKeyRepeatActions)}\" is off).{Environment.NewLine}If you remove it the multiple actions it represents will also be removed.");

            if (result != ContentDialogResult.Primary) return;
        }

        foreach (var action in selectedActions)
        {
            // TODO: wait actions could be merged by TryRemove, which means they dont exist anymore. implement TryRemoveRange or something
            string? errorMsg = _actionCollection.TryRemove(action, mergeWaitActions: false);
            if (errorMsg is not null)
            {
                await _contentDialogService.ShowErrorDialog("Failed to remove action", errorMsg);
            }
        }
    }

    private IEnumerable<InputAction> GetSelectedActions()
    {
        var actions = ShowKeyRepeatActions ? _actionCollection.Actions : _actionCollection.ActionsExlKeyRepeat;
        for (int i = 0; i < SelectedRanges!.Count; i++)
        {
            var range = SelectedRanges![i];
            for (int j = range.FirstIndex; j < range.LastIndex + 1; j++)
            {
                yield return actions[j];
            }
        }
    }
}
