using System;
using ActionRepeater.Win32.Input;
using ActionRepeater.Win32.WindowsAndMessages;
using ActionRepeater.Action;
using ActionRepeater.Utilities;
using System.Diagnostics;

namespace ActionRepeater.Input;

internal static class Recorder
{
    public static bool IsRecording { get; private set; } = false;

    public static Action<IInputAction> AddActionToList;
    public static Action<IInputAction> ReplaceLastAction;
    public static Func<IInputAction> GetLastAction;

    private static bool _isSubscribed = false;

    private static int _lastNewActionTickCount;
    private static readonly TimeConsistencyChecker _wheelMsgTCC = new();

    public static void Reset()
    {
        _lastNewActionTickCount = Environment.TickCount;
        _wheelMsgTCC.Reset();
    }

    public static void StartRecording()
    {
        if (!_isSubscribed)
        {
            //return;
            SubscribeToEvents();
        }

        Reset();

        IsRecording = true;
    }

    public static void StopRecording()
    {
        if (!_isSubscribed)
        {
            return;
        }

        UnsubscribeToEvents();
        IsRecording = false;
    }

    public static void SubscribeToEvents()
    {
        IntPtr handle = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        RAWINPUTDEVICE[] rid = new[]
        {
            new RAWINPUTDEVICE()
            {
                usUsagePage = UsagePage.GenericDesktopControl,
                usUsage = UsageId.Mouse,
                dwFlags = RawInputFlags.INPUTSINK,
                hwndTarget = handle
            },
            new RAWINPUTDEVICE()
            {
                usUsagePage = UsagePage.GenericDesktopControl,
                usUsage = UsageId.Keyboard,
                dwFlags = RawInputFlags.INPUTSINK,
                hwndTarget = handle
            }
        };

        if (!Win32.PInvoke.RegisterRawInputDevices(rid))
        {
            throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastPInvokeError());
        }

        _isSubscribed = true;
    }

    public static void UnsubscribeToEvents()
    {
        RAWINPUTDEVICE[] rid = new[]
        {
            new RAWINPUTDEVICE()
            {
                usUsagePage = UsagePage.GenericDesktopControl,
                usUsage = UsageId.Mouse,
                dwFlags = RawInputFlags.REMOVE,
                hwndTarget = IntPtr.Zero
            },
            new RAWINPUTDEVICE()
            {
                usUsagePage = UsagePage.GenericDesktopControl,
                usUsage = UsageId.Keyboard,
                dwFlags = RawInputFlags.REMOVE,
                hwndTarget = IntPtr.Zero
            }
        };

        if (!Win32.PInvoke.RegisterRawInputDevices(rid))
        {
            throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastPInvokeError());
        }

        _isSubscribed = false;
    }

    /// <summary>
    /// Callback for window message proc. should not be manually called.
    /// </summary>
    public static void OnMessageReceived(object sender, Messaging.WindowMessageEventArgs e)
    {
        if (e.MessageType != WindowMessage.INPUT) return;

        var inputCode = unchecked(e.Message.wParam & 0xff);
        if (!Win32.PInvoke.GetRawInputData(e.Message.lParam, out RAWINPUT inputData))
        {
            Debug.WriteLine("ERROR RETRIEVING RAW INPUT");
        }

        switch (inputData.header.dwType)
        {
            case RawInputType.Mouse:
                RAWMOUSE data = inputData.data.mouse;

                if (data.rawButtonData.buttonInfo.usButtonFlags == 0) // move event
                {
                    //Debug.WriteLine($"{data.usFlags} - {data.lLastX}, {data.lLastY}\n");
                }
                else // button/wheel event
                {
                    //Debug.WriteLine($"{data.rawButtonData.buttonInfo.usButtonFlags} - {data.rawButtonData.buttonInfo.usButtonData}\n");
                    var buttonInfo = data.rawButtonData.buttonInfo;
                    switch (buttonInfo.usButtonFlags)
                    {
                        case RawMouseButtonState.HWHEEL:
                        case RawMouseButtonState.WHEEL:
                            int wheelStepCount = unchecked((short)buttonInfo.usButtonData) / 120;

                            if (_wheelMsgTCC.UpdateAndCheckIfConsistent()
                                && GetLastAction() is MouseWheelAction lastMWAction)
                            {
                                int prevWheelStepCount = lastMWAction.StepCount;

                                if ((wheelStepCount < 0) == (prevWheelStepCount < 0))
                                {
                                    lastMWAction.StepCount = prevWheelStepCount + wheelStepCount;
                                    lastMWAction.Duration = _wheelMsgTCC.TickDeltasTotal;
                                    break;
                                }
                            }

                            AddAction(new MouseWheelAction(buttonInfo.usButtonFlags == RawMouseButtonState.HWHEEL, wheelStepCount));
                            break;

                        case RawMouseButtonState.LEFT_BUTTON_DOWN:
                            if (App.MainWindow.IsHoveringOverExcl) break;
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonDown, InputSimulator.MouseButton.Left,
                                Win32.PInvoke.Helpers.GetCursorPos(), App.MainWindow.UseCursorPosOnClicks));
                            break;
                        case RawMouseButtonState.LEFT_BUTTON_UP:
                            if (App.MainWindow.IsHoveringOverExcl) break;
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonUp, InputSimulator.MouseButton.Left,
                                Win32.PInvoke.Helpers.GetCursorPos(), App.MainWindow.UseCursorPosOnClicks));
                            break;

                        case RawMouseButtonState.RIGHT_BUTTON_DOWN:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonDown, InputSimulator.MouseButton.Right,
                                Win32.PInvoke.Helpers.GetCursorPos(), App.MainWindow.UseCursorPosOnClicks));
                            break;
                        case RawMouseButtonState.RIGHT_BUTTON_UP:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonUp, InputSimulator.MouseButton.Right,
                                Win32.PInvoke.Helpers.GetCursorPos(), App.MainWindow.UseCursorPosOnClicks));
                            break;

                        case RawMouseButtonState.MIDDLE_BUTTON_DOWN:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonDown, InputSimulator.MouseButton.Middle,
                                Win32.PInvoke.Helpers.GetCursorPos(), App.MainWindow.UseCursorPosOnClicks));
                            break;
                        case RawMouseButtonState.MIDDLE_BUTTON_UP:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonUp, InputSimulator.MouseButton.Middle,
                                Win32.PInvoke.Helpers.GetCursorPos(), App.MainWindow.UseCursorPosOnClicks));
                            break;

                        case RawMouseButtonState.XBUTTON1_DOWN:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonDown, InputSimulator.MouseButton.X1,
                                Win32.PInvoke.Helpers.GetCursorPos(), App.MainWindow.UseCursorPosOnClicks));
                            break;
                        case RawMouseButtonState.XBUTTON1_UP:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonUp, InputSimulator.MouseButton.X1,
                                Win32.PInvoke.Helpers.GetCursorPos(), App.MainWindow.UseCursorPosOnClicks));
                            break;

                        case RawMouseButtonState.XBUTTON2_DOWN:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonDown, InputSimulator.MouseButton.X2,
                                Win32.PInvoke.Helpers.GetCursorPos(), App.MainWindow.UseCursorPosOnClicks));
                            break;
                        case RawMouseButtonState.XBUTTON2_UP:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonUp, InputSimulator.MouseButton.X2,
                                Win32.PInvoke.Helpers.GetCursorPos(), App.MainWindow.UseCursorPosOnClicks));
                            break;
                    }
                }

                break;

            case RawInputType.Keyboard:
                VirtualKey key = inputData.data.keyboard.VKey;
                if (key == VirtualKey.NO_KEY || (ushort)key == 255)
                {
                    break;
                }

                var keyFlags = inputData.data.keyboard.Flags;
                if (keyFlags.HasFlag(RawInputKeyFlags.BREAK))
                {
                    AddAction(new KeyAction(KeyAction.Type.KeyUp, key));
                }
                else //if (keyFlags.HasFlag(RawInputKeyFlags.MAKE))
                {
                    var actions = App.MainWindow.Actions;
                    bool isAutoRepeat = false;
                    for (int i = actions.Count - 1; i > -1; --i)
                    {
                        if (actions[i] is KeyAction keyActionI
                            && keyActionI.Key == key)
                        {
                            isAutoRepeat = keyActionI.ActionType == KeyAction.Type.KeyDown;
                            break;
                        }
                    }

                    AddAction(new KeyAction(KeyAction.Type.KeyDown, key, isAutoRepeat));
                }

                //Debug.WriteLine($"{key} - E0: {keyFlags.HasFlag(RawInputKeyFlags.E0)}  E1: {keyFlags.HasFlag(RawInputKeyFlags.E1)}");
                //Debug.WriteLine(Convert.ToString((ushort)keyFlags, 2).PadLeft(4, '0'));
                break;
        }

        if (inputCode == 0)
        {
            e.Handled = true;
            e.Result = Win32.PInvoke.DefWindowProc(e.Message.hwnd, e.Message.message, e.Message.wParam, e.Message.lParam);
        }
    }

    private static void AddAction(IInputAction action)
    {
        if (action is null)
        {
            //_lastNewActionTickCount = Environment.TickCount;
            return;
        }

        if (action is not MouseWheelAction)
        {
            _wheelMsgTCC.Reset();
        }

        int curTickCount = Environment.TickCount;
        int ticksSinceLastAction = curTickCount - _lastNewActionTickCount;

        if (ticksSinceLastAction <= App.MainWindow.MaxClickInterval)
        {
            if (action is KeyAction curKeyAction
                && curKeyAction.ActionType == KeyAction.Type.KeyUp
                && GetLastAction() is KeyAction lastKeyAction
                && lastKeyAction.ActionType == KeyAction.Type.KeyDown
                && !lastKeyAction.IsAutoRepeat
                && lastKeyAction.Key == curKeyAction.Key)
            {
                ReplaceLastAction(new KeyAction(KeyAction.Type.KeyPress, curKeyAction.Key));

                _lastNewActionTickCount = curTickCount;
                return;
            }
            else if (action is MouseButtonAction curMBAction
                && curMBAction.ActionType == MouseButtonAction.Type.MouseButtonUp
                && GetLastAction() is MouseButtonAction lastMBAction
                && lastMBAction.ActionType == MouseButtonAction.Type.MouseButtonDown
                && lastMBAction.Button == curMBAction.Button
                && lastMBAction.Position == curMBAction.Position)
            {
                ReplaceLastAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonClick, curMBAction.Button, curMBAction.Position, curMBAction.UsePosition));

                _lastNewActionTickCount = curTickCount;
                return;
            }
        }

        if (ticksSinceLastAction > 10)
        {
            AddActionToList(new WaitAction(ticksSinceLastAction));
        }

        AddActionToList(action);
        App.MainWindow.ScrollActionList();

        _lastNewActionTickCount = curTickCount;
    }
}
