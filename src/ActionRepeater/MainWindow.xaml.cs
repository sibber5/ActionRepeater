using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ActionRepeater.Action;
using ActionRepeater.Input;
using ActionRepeater.Messaging;

#pragma warning disable RCS1163, IDE0060

namespace ActionRepeater;

public sealed partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly WindowMessageMonitor _msgMonitor;

    public bool IsHoveringOverExcl => NavViewCmdBar.SelectedItem is HomePage home && home.IsHoveringOverExcl;

    public bool IsPlayingActions { get; private set; }

    public ObservableCollection<IInputAction> Actions { get; }
    private readonly ObservableCollection<IInputAction> _actionsEclKeyRepeat;
    private ObservableCollection<IInputAction> FilteredActions { get => ShowAutoRepeatActionsToggle.IsOn ? Actions : _actionsEclKeyRepeat; }

    public CursorTrackingMode CursorTrackingMode { get; internal set; } = CursorTrackingMode.None;

    private bool _useCursorPosOnClicks = true;
    public bool UseCursorPosOnClicks
    {
        get => _useCursorPosOnClicks;
        set
        {
            if (_useCursorPosOnClicks == value) return;

            foreach (IInputAction action in Actions)
            {
                if (action is MouseButtonAction mbAction)
                {
                    mbAction.UsePosition = value;
                }
            }
            _useCursorPosOnClicks = value;
            UpdatePropertyInView();
        }
    }

    public int MaxClickInterval { get; internal set; } = 120;

    private bool _sendKeyAutoRepeat = true;
    public bool SendKeyAutoRepeat
    {
        get => _sendKeyAutoRepeat;
        set
        {
            _sendKeyAutoRepeat = value;
            UpdatePropertyInView();
        }
    }

    private IInputAction _copiedAction = null;

    public MainWindow()
    {
        Actions = new();
        _actionsEclKeyRepeat = new();

        IsPlayingActions = Player.IsPlaying;
        Player.IsPlayingChanged += (s, isPlayingNewVal) =>
        {
            IsPlayingActions = isPlayingNewVal;
            UpdatePropertyInView(nameof(IsPlayingActions));

            if (NavViewCmdBar.SelectedItem is HomePage home) home.IsRecordButtonEnabled = !isPlayingNewVal;
        };

        Recorder.AddActionToList = AddAction;
        Recorder.ReplaceLastAction = (newAction) =>
        {
            if (Actions[^1] == _actionsEclKeyRepeat[^1])
            {
                _actionsEclKeyRepeat[^1] = newAction;
            }
            Actions[^1] = newAction;
        };
        Recorder.GetLastAction = () =>
        {
            if (Actions.Count > 0) return Actions[^1];
            return null;
        };

        InitializeComponent();
        Closed += OnClosed;
        SizeChanged += OnSizeChanged;

        _msgMonitor = new WindowMessageMonitor(this);
        _msgMonitor.WindowMessageReceived += Recorder.OnMessageReceived;
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        if (Recorder.IsRecording)
        {
            Recorder.StopRecording();
        }
        _msgMonitor.Dispose();
    }

    private void OnSizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        switch (args.Size.Height)
        {
            case < 160:
                NavViewCmdBar.Visibility = Visibility.Collapsed;
                ActionListHeader.Visibility = Visibility.Collapsed;
                break;

            case > 160:
                NavViewCmdBar.Visibility = Visibility.Visible;
                ActionListHeader.Visibility = Visibility.Visible;
                break;
        }
    }

    private void NavViewCmdBar_Loaded(object sender, RoutedEventArgs e)
    {
        NavViewCmdBar.SelectedItem = NavViewCmdBar.MenuItems[0];
        NavigateCommandBar("home", new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
    }

    private void NavViewCmdBar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        NavigateCommandBar(args.SelectedItemContainer.Tag.ToString(), args.RecommendedNavigationTransitionInfo);
    }

    private void NavigateCommandBar(string tag, Microsoft.UI.Xaml.Media.Animation.NavigationTransitionInfo navInfo)
    {
        System.Type pageType = tag switch
        {
            "home" => typeof(HomePage),
            "opts" => typeof(OptionsPage),
            _ => null
        };
        if (pageType is not null && !Equals(NavViewContent.CurrentSourcePageType, pageType))
        {
            NavViewContent.Navigate(pageType, null, navInfo);
        }
    }

    public void ScrollActionList() => ActionList.ScrollIntoView(ActionList.Items[^1]);

    public void AddAction(IInputAction action)
    {
        WaitAction newWaitAction = action as WaitAction;

        if (newWaitAction is not null && Actions.Count > 0 && Actions[^1] is WaitAction lastWaitAction)
        {
            lastWaitAction.Duration += newWaitAction.Duration;
        }
        else
        {
            Actions.Add(action);
        }

        if (!(action is KeyAction keyAction && keyAction.IsAutoRepeat))
        {
            if (newWaitAction is not null && _actionsEclKeyRepeat.Count > 0 && _actionsEclKeyRepeat[^1] is WaitAction lastFilteredWaitAction)
            {
                lastFilteredWaitAction.Duration += newWaitAction.Duration;
            }
            else
            {
                _actionsEclKeyRepeat.Add(action);
            }
        }
    }

    private void ShowAutoRepeatActions_Toggled(object sender, RoutedEventArgs e) => UpdatePropertyInView(nameof(FilteredActions));

    private void ActionList_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var listView = (ListView)sender;
        var actionItem = (IInputAction)((FrameworkElement)e.OriginalSource).DataContext;

        if (actionItem is null)
        {
            ActionListMenuFlyout.Items[0].IsEnabled = _copiedAction is not null;
            ActionListMenuFlyout.ShowAt(listView, e.GetPosition(listView));
            return;
        }

        ActionList.SelectedItem = actionItem;

        ActionItemMenuFlyout.Items[1].IsEnabled = _copiedAction is not null;
        ActionItemMenuFlyout.ShowAt(listView, e.GetPosition(listView));
    }

    private void OnCopyClick(object sender, RoutedEventArgs e) => _copiedAction = (IInputAction)ActionList.SelectedItem;

    private void OnRemoveClick(object sender, RoutedEventArgs e)
    {
        var selectedItem = (IInputAction)ActionList.SelectedItem;
        Actions.Remove(selectedItem);
        _actionsEclKeyRepeat.Remove(selectedItem);
    }

    private void OnReplaceClick(object sender, RoutedEventArgs e)
    {
        var newItem = _copiedAction.Clone();
        IInputAction selectedItem;
        int selectedItemIdx;

        if (ShowAutoRepeatActionsToggle.IsOn)
        {
            selectedItem = Actions[ActionList.SelectedIndex];

            Actions[ActionList.SelectedIndex] = newItem;

            selectedItemIdx = _actionsEclKeyRepeat.IndexOf(selectedItem);
            _actionsEclKeyRepeat[selectedItemIdx] = newItem;

            return;
        }

        selectedItem = _actionsEclKeyRepeat[ActionList.SelectedIndex];

        _actionsEclKeyRepeat[ActionList.SelectedIndex] = newItem;

        selectedItemIdx = Actions.IndexOf(selectedItem);
        Actions[selectedItemIdx] = newItem;
    }

    private void OnPasteClick(object sender, RoutedEventArgs e)
    {
        AddAction(_copiedAction.Clone());
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        Actions.Clear();
        _actionsEclKeyRepeat.Clear();
        Recorder.Reset();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void UpdatePropertyInView([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public enum CursorTrackingMode
{
    None = 0,
    Absolute,
    Relative
}
