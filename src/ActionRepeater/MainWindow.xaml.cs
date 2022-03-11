using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ActionRepeater.Action;
using ActionRepeater.Input;
using ActionRepeater.Messaging;
using System.Diagnostics;

namespace ActionRepeater;

public sealed partial class MainWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void UpdatePropertyInView([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public IntPtr Handle { get; }

    private readonly WindowMessageMonitor _msgMonitor;

    public bool IsHoveringOverExcl => NavViewFrame.Content is HomePage home && home.IsHoveringOverExcl;

    public bool IsPlayingActions { get; private set; }

    public ObservableCollection<InputAction> Actions { get; }
    public ObservableCollection<InputAction> ActionsEclKeyRepeat { get; }
    private ObservableCollection<InputAction> FilteredActions { get => ShowAutoRepeatToggle.IsOn ? Actions : ActionsEclKeyRepeat; }

    public CursorTrackingMode CursorTrackingMode { get; internal set; } = CursorTrackingMode.None;

    private bool _useCursorPosOnClicks = true;
    public bool UseCursorPosOnClicks
    {
        get => _useCursorPosOnClicks;
        set
        {
            if (_useCursorPosOnClicks == value) return;

            foreach (InputAction action in Actions)
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

    private readonly List<int> _modifiedFilteredActionIdx = new();

    private InputAction? _copiedAction;

    public MainWindow()
    {
        Handle = WinRT.Interop.WindowNative.GetWindowHandle(this);

        Actions = new();
        ActionsEclKeyRepeat = new();

        IsPlayingActions = Player.IsPlaying;
        Player.IsPlayingChanged += (s, isPlayingNewVal) =>
        {
            IsPlayingActions = isPlayingNewVal;
            UpdatePropertyInView(nameof(IsPlayingActions));

            if (NavViewFrame.Content is HomePage home) home.IsRecordButtonEnabled = !isPlayingNewVal;
        };

        Recorder.AddActionToList = AddAction;
        Recorder.ReplaceLastAction = (newAction) =>
        {
            // the caller of this func always checks if the action list is not empty
            if (Actions[^1] == ActionsEclKeyRepeat[^1])
            {
                ActionsEclKeyRepeat[^1] = newAction;
            }
            Actions[^1] = newAction;
        };
        Recorder.GetLastAction = () =>
        {
            if (Actions.Count > 0) return Actions[^1];
            return null;
        };

        InitializeComponent();

        _msgMonitor = new WindowMessageMonitor(this);
        _msgMonitor.WindowMessageReceived += Recorder.OnMessageReceived;
    }

    // when changing/updating this method also update FillFilteredActionList if required
    public void AddAction(InputAction action)
    {
        if (action is WaitAction waitAction)
        {
            if (Actions.Count > 0 && Actions[^1] is WaitAction lastWaitAction)
            {
                lastWaitAction.Duration += waitAction.Duration;
            }
            else
            {
                Actions.Add(action);
            }

            int lastFilteredActionIdx = ActionsEclKeyRepeat.Count - 1;
            if (ActionsEclKeyRepeat.Count > 0 && ActionsEclKeyRepeat[lastFilteredActionIdx] is WaitAction lastFilteredWaitAction)
            {
                if (_modifiedFilteredActionIdx.Contains(lastFilteredActionIdx))
                {
                    lastFilteredWaitAction.Duration += waitAction.Duration;
                }
                else if (!ReferenceEquals(Actions[^1], lastFilteredWaitAction))
                {
                    ActionsEclKeyRepeat[lastFilteredActionIdx] = new WaitAction(lastFilteredWaitAction.Duration + waitAction.Duration);
                    _modifiedFilteredActionIdx.Add(lastFilteredActionIdx);
                }
            }
            else
            {
                ActionsEclKeyRepeat.Add(action);
            }

            return;
        }

        Actions.Add(action);

        if (!(action is KeyAction keyAction && keyAction.IsAutoRepeat))
        {
            ActionsEclKeyRepeat.Add(action);
        }
    }

    /// <summary>
    /// Fills ActionsEclKeyRepeat from the actinos in Actions.
    /// </summary>
    public void FillFilteredActionList()
    {
        ActionsEclKeyRepeat.Clear();
        _modifiedFilteredActionIdx.Clear();

        for (int i = 0; i < Actions.Count; ++i)
        {
            InputAction action = Actions[i];

            if (action is WaitAction waitAction)
            {
                int lastFilteredActionIdx = ActionsEclKeyRepeat.Count - 1;
                if (ActionsEclKeyRepeat.Count > 0 && ActionsEclKeyRepeat[lastFilteredActionIdx] is WaitAction lastFilteredWaitAction)
                {
                    if (_modifiedFilteredActionIdx.Contains(lastFilteredActionIdx))
                    {
                        lastFilteredWaitAction.Duration += waitAction.Duration;
                    }
                    else if (!ReferenceEquals(Actions[i - 1], lastFilteredWaitAction))
                    {
                        ActionsEclKeyRepeat[lastFilteredActionIdx] = new WaitAction(lastFilteredWaitAction.Duration + waitAction.Duration);
                        _modifiedFilteredActionIdx.Add(lastFilteredActionIdx);
                    }
                }
                else
                {
                    ActionsEclKeyRepeat.Add(action);
                }

                continue;
            }

            if (!(action is KeyAction keyAction && keyAction.IsAutoRepeat))
            {
                ActionsEclKeyRepeat.Add(action);
            }
        }
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
                ShowAutoRepeatLabel.Visibility = Visibility.Collapsed;
                ShowAutoRepeatToggle.Visibility = Visibility.Collapsed;
                break;

            case > 160:
                NavViewCmdBar.Visibility = Visibility.Visible;
                ActionListHeader.Visibility = Visibility.Visible;
                ShowAutoRepeatLabel.Visibility = Visibility.Visible;
                ShowAutoRepeatToggle.Visibility = Visibility.Visible;
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

    private void NavigateCommandBar(string? tag, Microsoft.UI.Xaml.Media.Animation.NavigationTransitionInfo navInfo)
    {
        Type? pageType = tag switch
        {
            "home" => typeof(HomePage),
            "opts" => typeof(OptionsPage),
            _ => null
        };
        if (pageType is not null && !Equals(NavViewFrame.CurrentSourcePageType, pageType))
        {
            NavViewFrame.Navigate(pageType, null, navInfo);
        }
    }

    public void ScrollActionList() => ActionList.ScrollIntoView(ActionList.Items[^1]);

    private void ShowAutoRepeatActions_Toggled(object sender, RoutedEventArgs e) => UpdatePropertyInView(nameof(FilteredActions));

    private void ActionList_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var listView = (ListView)sender;
        var actionItem = (InputAction)((FrameworkElement)e.OriginalSource).DataContext;

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

    private void OnCopyClick(object sender, RoutedEventArgs e) => _copiedAction = (InputAction)ActionList.SelectedItem;

    private async void OnRemoveClick(object sender, RoutedEventArgs e)
    {
        var selectedItem = (InputAction)ActionList.SelectedItem;

        foreach (int idx in _modifiedFilteredActionIdx)
        {
            if (ActionsEclKeyRepeat[idx] == selectedItem)
            {
                await new ContentDialog()
                {
                    XamlRoot = ActionList.XamlRoot,
                    Title = "⚠ Failed to remove action",
                    Content = $"This action represents multiple hidden actions (because \"{ShowAutoRepeatLabel.Text}\" is off).\nRemoving it will result in unexpected behavior.",
                    CloseButtonText = "Ok"
                }.ShowAsync();

                return;
            }
        }

        Actions.Remove(selectedItem);
        ActionsEclKeyRepeat.Remove(selectedItem);
    }

    private void OnReplaceClick(object sender, RoutedEventArgs e)
    {
        var newItem = _copiedAction!.Clone();
        InputAction selectedItem;
        int selectedItemIdx;

        if (ShowAutoRepeatToggle.IsOn)
        {
            selectedItem = Actions[ActionList.SelectedIndex];

            Actions[ActionList.SelectedIndex] = newItem;

            selectedItemIdx = ActionsEclKeyRepeat.IndexOf(selectedItem);
            ActionsEclKeyRepeat[selectedItemIdx] = newItem;

            return;
        }

        selectedItem = ActionsEclKeyRepeat[ActionList.SelectedIndex];

        ActionsEclKeyRepeat[ActionList.SelectedIndex] = newItem;

        selectedItemIdx = Actions.IndexOf(selectedItem);
        Actions[selectedItemIdx] = newItem;
    }

    private void OnPasteClick(object sender, RoutedEventArgs e)
    {
        AddAction(_copiedAction!.Clone());
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        Actions.Clear();
        ActionsEclKeyRepeat.Clear();
        _modifiedFilteredActionIdx.Clear();
        Recorder.Reset();
    }
}

public enum CursorTrackingMode
{
    None = 0,
    Absolute,
    Relative
}
