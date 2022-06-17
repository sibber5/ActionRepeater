using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Commands;
using ActionRepeater.UI.Utilities;

namespace ActionRepeater.UI.ViewModels;

public class ActionListViewModel : ViewModelBase
{
    public Func<string, string, Task>? ShowErrorDialog { get; set; }

    private bool _showKeyRepeatActions = false;
    public bool ShowKeyRepeatActions
    {
        get => _showKeyRepeatActions;
        set
        {
            _showKeyRepeatActions = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(FilteredActions));
        }
    }

    public ObservableCollection<InputAction> FilteredActions { get => ShowKeyRepeatActions ? ActionManager.Actions : ActionManager.ActionsExlKeyRepeat; }

    private InputAction? _selectedAction;
    public InputAction? SelectedAction
    {
        get => _selectedAction;
        set
        {
            if (_selectedAction == value) return;

            _selectedAction = value;
            RaisePropertyChanged();
        }
    }

    private int _selectedActionIndex = -1;
    public int SelectedActionIndex
    {
        get => _selectedActionIndex;
        set
        {
            if (_selectedActionIndex == value) return;

            _selectedActionIndex = value;
            RaisePropertyChanged();
        }
    }

    public ICommand CopyCommand { get; }
    public ICommand PasteCommand { get; }
    public ICommand ReplaceCommand { get; }
    public ICommand RemoveCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand ClearActionsCommand { get; }
    public ICommand ClearCursorPathCommand { get; }

    public ActionListViewModel(ActionHolder copiedActionHolder)
    {
        CopyCommand = new StoreActionCommand(copiedActionHolder, this);
        PasteCommand = new AddActionCommand(copiedActionHolder);
        ReplaceCommand = new ReplaceActionCommand(copiedActionHolder, this);
        RemoveCommand = new RemoveActionCommand(this);
        ClearCommand = new ClearActionsAndCursorPathCommand();
        ClearActionsCommand = new ClearActionsCommand();
        ClearCursorPathCommand = new ClearCursorPathCommand();
    }
}
