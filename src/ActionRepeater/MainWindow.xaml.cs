using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
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
    private void UpdatePropertyInView(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public IntPtr Handle { get; }

    public bool IsHoveringOverExcl => NavViewFrame.Content is HomePage home && home.IsHoveringOverExcl;

    public bool IsPlayingActions { get; private set; }

    private ObservableCollection<InputAction> FilteredActions { get => ShowAutoRepeatToggle.IsOn ? ActionManager.Actions : ActionManager.ActionsExlKeyRepeat; }

    private readonly WindowMessageMonitor _msgMonitor;

    private InputAction? _copiedAction;

    public MainWindow()
    {
        Handle = WinRT.Interop.WindowNative.GetWindowHandle(this);

        IsPlayingActions = Player.IsPlaying;
        Player.IsPlayingChanged += (s, isPlayingNewVal) =>
        {
            IsPlayingActions = isPlayingNewVal;
            UpdatePropertyInView(nameof(IsPlayingActions));

            if (NavViewFrame.Content is HomePage home) home.IsRecordButtonEnabled = !isPlayingNewVal;
        };

        InitializeComponent();

        _msgMonitor = new WindowMessageMonitor(this);
        _msgMonitor.WindowMessageReceived += OnWindowMessage;
    }

    private void OnWindowMessage(object? sender, WindowMessageEventArgs e)
    {
        if (e.MessageType == Win32.WindowsAndMessages.WindowMessage.INPUT)
        {
            Recorder.OnInputMessage(e);
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

    public void ScrollActionList()
    {
        ActionList.ScrollIntoView(ActionList.Items[^1]);
    }

    private void ShowAutoRepeatActions_Toggled(object sender, RoutedEventArgs e)
    {
        UpdatePropertyInView(nameof(FilteredActions));
    }

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

    private void OnCopyClick(object sender, RoutedEventArgs e)
    {
        _copiedAction = (InputAction)ActionList.SelectedItem;
    }

    private async void OnRemoveClick(object sender, RoutedEventArgs e)
    {
        var selectedItem = (InputAction)ActionList.SelectedItem;

        if (!ActionManager.TryRemoveAction(selectedItem))
        {
            await new ContentDialog()
            {
                XamlRoot = ActionList.XamlRoot,
                Title = "⚠ Failed to remove action",
                Content = $"This action represents multiple hidden actions (because \"{ShowAutoRepeatLabel.Text}\" is off).{Environment.NewLine}Removing it will result in unexpected behavior.",
                CloseButtonText = "Ok"
            }.ShowAsync();
        }
    }

    private void OnReplaceClick(object sender, RoutedEventArgs e)
    {
        ActionManager.ReplaceAction(!ShowAutoRepeatToggle.IsOn, ActionList.SelectedIndex, _copiedAction!.Clone());
    }

    private void OnPasteClick(object sender, RoutedEventArgs e)
    {
        ActionManager.AddAction(_copiedAction!.Clone());
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        ActionManager.ClearActions();
        Recorder.Reset();
    }
}
