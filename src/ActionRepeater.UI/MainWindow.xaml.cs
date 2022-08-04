using System;
using Microsoft.UI.Xaml;
using ActionRepeater.UI.ViewModels;
using ActionRepeater.Win32.WindowsAndMessages.Utilities;
using ActionRepeater.Core.Input;
using Microsoft.UI.Xaml.Controls;
using ActionRepeater.UI.Pages;

namespace ActionRepeater.UI;

public sealed partial class MainWindow : Window
{
    public IntPtr Handle { get; }

    // set on init in App.OnLaunched
    public MainViewModel ViewModel { get; set; } = null!;

    internal XamlRoot GridXamlRoot => _grid.XamlRoot;

    internal const string HomeTag = "h";
    internal const string OptionsTag = "o";

    private readonly WindowMessageMonitor _msgMonitor;

    public MainWindow()
    {
        Handle = WinRT.Interop.WindowNative.GetWindowHandle(this);

        _msgMonitor = new(Handle);
        _msgMonitor.WindowMessageReceived += OnWindowMessageReceived;

        Player.ExecuteOnUIThread = (action) => DispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(action));

        if (App.Current.RequestedTheme == ApplicationTheme.Dark)
        {
            Win32.PInvoke.Helpers.SetWindowImmersiveDarkMode(Handle, true);
        }

        InitializeComponent();
    }

    private static void OnWindowMessageReceived(object? sender, WindowMessageEventArgs e)
    {
        if (e.MessageType == Win32.WindowsAndMessages.WindowMessage.INPUT)
        {
            Recorder.OnInputMessage(e);
        }
    }

    private void NavigationView_Loaded(object sender, RoutedEventArgs e)
    {
        _navigationView.SelectedItem = _navigationView.MenuItems[1];
        Navigate(HomeTag, new Microsoft.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo());
    }

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        Navigate((string?)args.SelectedItemContainer.Tag, args.RecommendedNavigationTransitionInfo);
    }

    private void Navigate(string? tag, Microsoft.UI.Xaml.Media.Animation.NavigationTransitionInfo navInfo)
    {
        switch (tag)
        {
            case HomeTag:
                _contentFrame.Navigate(typeof(HomePage), ViewModel.HomeViewModel, navInfo);
                break;

            case OptionsTag:
                _contentFrame.Navigate(typeof(OptionsPage), ViewModel.OptionsViewModel, navInfo);
                break;
        }
    }
}
