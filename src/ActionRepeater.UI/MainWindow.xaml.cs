using System;
using Microsoft.UI.Xaml;
using ActionRepeater.UI.ViewModels;

namespace ActionRepeater.UI;

public sealed partial class MainWindow : Window
{
    public IntPtr Handle { get; }

    // set on init in App.OnLaunched
    internal MainViewModel ViewModel { get; init; } = null!;

    public MainWindow()
    {
        Handle = WinRT.Interop.WindowNative.GetWindowHandle(this);

        InitializeComponent();
    }
}
