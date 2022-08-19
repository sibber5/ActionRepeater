using System;
using System.IO;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Pages;
using ActionRepeater.UI.ViewModels;
using ActionRepeater.Win32.WindowsAndMessages;
using ActionRepeater.Win32.WindowsAndMessages.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI;

public sealed partial class MainWindow : Window
{
    private const string WindowTitle = "ActionRepeater";
    private const int StartupWidth = 507;
    private const int StartupHeight = 700;
    private const int MinWidth = 356;
    private const int MinHeight = 206;

    public IntPtr Handle { get; }

    private readonly MainViewModel _viewModel;

    public XamlRoot GridXamlRoot => _grid.XamlRoot;

    public const string HomeTag = "h";
    public const string OptionsTag = "o";

    private readonly WindowMessageMonitor _msgMonitor;

    public MainWindow()
    {
        Handle = WinRT.Interop.WindowNative.GetWindowHandle(this);
        
        Title = WindowTitle;
        App.SetWindowSize(Handle, StartupWidth, StartupHeight);
        
        // set window icon
        var windowId = Win32Interop.GetWindowIdFromWindow(Handle);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
        appWindow.SetIcon(Path.Combine(App.AppFolderDir, @"Assets\Icon.ico"));

        _msgMonitor = new(Handle);
        _msgMonitor.WindowMessageReceived += OnWindowMessageReceived;

        if (App.Current.RequestedTheme == ApplicationTheme.Dark)
        {
            Win32.PInvoke.Helpers.SetWindowImmersiveDarkMode(Handle, true);
        }

        _viewModel = App.Current.Services.GetRequiredService<MainViewModel>();

        InitializeComponent();
    }

    private unsafe void OnWindowMessageReceived(object? sender, WindowMessageEventArgs e)
    {
        switch (e.MessageType)
        {
            case WindowMessage.INPUT:
                Recorder.OnInputMessage(e);
                break;

            case WindowMessage.GETMINMAXINFO:
                float scalingFactor = App.GetWindowScalingFactor(Handle);
                MINMAXINFO* info = (MINMAXINFO*)e.Message.lParam;
                info->ptMinTrackSize.x = (int)(MinWidth * scalingFactor);
                info->ptMinTrackSize.y = (int)(MinHeight * scalingFactor);
                break;
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
                _contentFrame.Navigate(typeof(HomePage), null, navInfo);
                break;

            case OptionsTag:
                _contentFrame.Navigate(typeof(OptionsPage), null, navInfo);
                break;
        }
    }
}
