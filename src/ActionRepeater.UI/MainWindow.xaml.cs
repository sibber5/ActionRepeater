using System;
using Microsoft.UI.Xaml;
using ActionRepeater.UI.ViewModels;
using ActionRepeater.Win32.WindowsAndMessages.Utilities;
using ActionRepeater.Core.Input;

namespace ActionRepeater.UI;

public sealed partial class MainWindow : Window
{
    public IntPtr Handle { get; }

    // set on init in App.OnLaunched
    internal MainViewModel ViewModel { get; set; } = null!;

    internal XamlRoot GridXamlRoot => _grid.XamlRoot;

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

    private void OnWindowMessageReceived(object? sender, WindowMessageEventArgs e)
    {
        if (e.MessageType == Win32.WindowsAndMessages.WindowMessage.INPUT)
        {
            Recorder.OnInputMessage(e);
        }
    }
}
