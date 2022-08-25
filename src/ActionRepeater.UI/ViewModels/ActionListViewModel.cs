using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Services;
using ActionRepeater.UI.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;

namespace ActionRepeater.UI.ViewModels;

public partial class ActionListViewModel : ObservableObject
{
    private readonly System.ComponentModel.PropertyChangedEventArgs _actionListHeaderChangedArgs = new(nameof(ActionListHeaderWithCount));
    public string ActionListHeaderWithCount => $"Actions ({FilteredActions.Count}):";

    [NotifyPropertyChangedFor(nameof(FilteredActions))]
    [ObservableProperty]
    private bool _showKeyRepeatActions;

    internal SyncedObservableCollection<ActionViewModel, InputAction> ActionVMs { get; }
    internal SyncedObservableCollection<ActionViewModel, InputAction> ActionsExlVMs { get; }
    public IReadOnlyList<ActionViewModel> FilteredActions => ShowKeyRepeatActions ? ActionVMs : ActionsExlVMs;

    public InputAction? SelectedAction => SelectedActionIndex == -1
        ? null
        : (ShowKeyRepeatActions ? _actionCollection.Actions[SelectedActionIndex] : _actionCollection.ActionsExlKeyRepeat[SelectedActionIndex]);

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
            AddStoredActionCommand.NotifyCanExecuteChanged();
            ReplaceActionCommand.NotifyCanExecuteChanged();
        }
    }

    public bool CanAddAction => !_recorder.IsRecording;

    internal Action? ScrollToSelectedItem { get; set; }

    private readonly ContentDialogService _contentDialogService;

    private readonly DispatcherQueueHandler _updateSelectedAction;

    private readonly ActionCollection _actionCollection;
    private readonly Recorder _recorder;

    public ActionListViewModel(ContentDialogService contentDialogService, ActionCollection actionCollection, Recorder recorder)
    {
        _contentDialogService = contentDialogService;
        _actionCollection = actionCollection;
        _recorder = recorder;

        _updateSelectedAction = () =>
        {
            SelectedActionIndex = CurrentSetActionIndex;
            ScrollToSelectedItem?.Invoke();
        };

        Func<InputAction?, ActionViewModel> createVM = static (model) => new ActionViewModel(model!);

        ActionVMs = new((ObservableCollection<InputAction?>)_actionCollection.Actions, createVM);
        ActionsExlVMs = new((ObservableCollection<InputAction?>)_actionCollection.ActionsExlKeyRepeat, createVM);

        ((System.ComponentModel.INotifyPropertyChanged)FilteredActions).PropertyChanged += (s, e) =>
        {
            if (nameof(FilteredActions.Count).Equals(e.PropertyName, StringComparison.Ordinal))
            {
                OnPropertyChanged(_actionListHeaderChangedArgs);
            }
        };
        _recorder.IsRecordingChanged += (_, _) => OnPropertyChanged(nameof(CanAddAction));
    }

    public void UpdateActionListIndex(bool skipAutoRepeat)
    {
        CurrentSetActionIndex++;

        if (skipAutoRepeat)
        {
            var filteredActions = ShowKeyRepeatActions ? _actionCollection.Actions : _actionCollection.ActionsExlKeyRepeat;
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

        App.Current.MainWindow.DispatcherQueue.TryEnqueue(_updateSelectedAction);
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
    private void AddStoredAction() => _actionCollection.Add(CopiedAction!.Clone());

    [RelayCommand(CanExecute = nameof(IsActionStored))]
    private async Task ReplaceAction()
    {
        string? errorMsg = _actionCollection.TryReplace(!ShowKeyRepeatActions, SelectedActionIndex, CopiedAction!.Clone());
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
            await _contentDialogService.ShowYesNoMessageDialog("Are you sure you want to remove this action?",
                $"This action represents multiple hidden actions (because \"{nameof(ShowKeyRepeatActions)}\" is off).{Environment.NewLine}If you remove it the multiple actions it represents will be removed.",
                onYesClick: () => _actionCollection.TryRemove(SelectedAction!));

            return;
        }

        string? errorMsg = _actionCollection.TryRemove(SelectedAction);
        if (errorMsg is not null)
        {
            await _contentDialogService.ShowErrorDialog("Failed to remove action", errorMsg);
        }
    }
}
