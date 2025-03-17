using System;
using System.Threading;
using ActionRepeater.Core.Extentions;
using ActionRepeater.UI.Extensions;
using ActionRepeater.UI.ViewModels;
using ActionRepeater.Win32;
using ActionRepeater.Win32.WindowsAndMessages;
using ActionRepeater.Win32.WindowsAndMessages.Utilities;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Graphics;
using Windows.UI;
using WinRT;
using Compositor = Windows.UI.Composition.Compositor;

namespace ActionRepeater.UI.AppWindows;

public sealed partial class PathDrawingControls : Window, IWindowWithHandle
{
    private readonly SizeInt32 _startupSize = new() { Width = 50, Height = 50 };

    public nint Handle { get; }

    private readonly WindowMessageMonitor _msgMonitor;

    private readonly PathDrawingControlsViewModel _vm;

    private bool _isHoldingGrab;
    private PointInt32 _prevWindowPos;
    private POINT _prevCursorPos;

    public PathDrawingControls(PathDrawingControlsViewModel vm)
    {
        _vm = vm;

        Handle = WinRT.Interop.WindowNative.GetWindowHandle(this);

        Title = nameof(PathDrawingControls).AddSpacesBetweenWords();
        this.SetSize(_startupSize);

        AppWindow.IsShownInSwitchers = false;

        var presenter = (OverlappedPresenter)AppWindow.Presenter;

        presenter.IsAlwaysOnTop = true;
        presenter.SetBorderAndTitleBar(false, false);
        presenter.IsResizable = false;
        presenter.IsMinimizable = false;
        presenter.IsMaximizable = false;

        SetDwmBlur();
        SetSystemBackdropColor(Color.FromArgb(0, 0, 0, 0));

        _msgMonitor = new(Handle);
        _msgMonitor.WindowMessageReceived += OnWindowMessageReceived;

        InitializeComponent();

        ((FrameworkElement)Content).Loaded += (s, e) =>
        {
            var contentWidth = (int)((FrameworkElement)s).ActualWidth;
            this.SetSize(new(contentWidth, _startupSize.Height));

            // place window at top center of primary screen
            var primaryScreenRect = DisplayArea.Primary.WorkArea;
            var scalingFactor = this.GetScalingFactor();
            PointInt32 startupPosition = new(
                primaryScreenRect.X + (primaryScreenRect.Width / 2) - (int)(scalingFactor * contentWidth / 2),
                primaryScreenRect.Y + (int)(scalingFactor * 12)
            );

            AppWindow.Move(startupPosition);
        };
    }

    private void SetSystemBackdropColor(Color color)
    {
        var brush = CompositorManager.Compositor.CreateColorBrush(color);
        this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>().SystemBackdrop = brush;
    }

    private void SetDwmBlur()
    {
        PInvoke.DwmExtendFrameIntoClientArea(Handle, new(0, 0, 0, 0));

        const uint DWM_BB_ENABLE = 0x00000001u;
        const uint DWM_BB_BLURREGION = 0x00000002u;

        using var region = PInvoke.CreateRectRgn_SafeHandle(-2, -2, -1, -1);
        PInvoke.DwmEnableBlurBehindWindow(Handle, new(fEnable: true, dwFlags: DWM_BB_ENABLE | DWM_BB_BLURREGION, hRgnBlur: region.DangerousGetHandle()));
    }

    private unsafe void OnWindowMessageReceived(object? sender, WindowMessageEventArgs e)
    {
        switch (e.MessageType)
        {
            case WindowMessage.ERASEBKGND:
                if (PInvoke.GetClientRect(Handle, out var rect))
                {
                    using var brush = PInvoke.CreateSolidBrush_SafeHandle(MACROS.RGB(0, 0, 0));
                    PInvoke.FillRect((nint)e.Message.wParam, rect, brush.DangerousGetHandle());
                    e.Result = 1;
                    e.Handled = true;
                }
                break;

            case WindowMessage.DWMCOMPOSITIONCHANGED:
                SetDwmBlur();
                e.Result = 0;
                e.Handled = true;
                break;
        }
    }

    private void GrabIcon_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var properties = e.GetCurrentPoint((UIElement)sender).Properties;
        if (!properties.IsLeftButtonPressed) return;

        ((UIElement)sender).CapturePointer(e.Pointer);

        _prevWindowPos = AppWindow.Position;
        _prevCursorPos = PInvoke.Helpers.GetCursorPos();

        _isHoldingGrab = true;
    }

    private void GrabIcon_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        ((UIElement)sender).ReleasePointerCaptures();
        _isHoldingGrab = false;
    }

    private void GrabIcon_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var properties = e.GetCurrentPoint((UIElement)sender).Properties;
        if (!properties.IsLeftButtonPressed) return;

        var cursorPos = PInvoke.Helpers.GetCursorPos();

        if (_isHoldingGrab)
        {
            AppWindow.Move(new(_prevWindowPos.X + cursorPos.x - _prevCursorPos.x, _prevWindowPos.Y + cursorPos.y - _prevCursorPos.y));
        }

        e.Handled = true;
    }

    private void AppBarToggleButton_Click(object sender, RoutedEventArgs e)
    {
        var senderButton = (AppBarToggleButton)sender;

        if (senderButton.IsChecked != true)
        {
            senderButton.IsChecked = true;
            return;
        }

        foreach (var control in ((Panel)Content).Children)
        {
            if (control is AppBarToggleButton button && button != senderButton) button.IsChecked = false; 
        }
    }
}

static class CompositorManager
{
    private static readonly Lazy<Compositor> _compositor = new(static () =>
    {
        using ManualResetEventSlim mre = new(false);

        Compositor compositor = null!;
        var dispatcherQueueController = Windows.System.DispatcherQueueController.CreateOnDedicatedThread();
        dispatcherQueueController.DispatcherQueue.TryEnqueue(Windows.System.DispatcherQueuePriority.High, () =>
        {
            compositor = new();
            mre.Set();
        });

        mre.Wait();

        return compositor;
    });

    public static Compositor Compositor => _compositor.Value;
}
