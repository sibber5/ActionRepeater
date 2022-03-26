using System;
using ActionRepeater.Win32.Input;
using ActionRepeater.Action;
using ActionRepeater.Utilities;
using System.Diagnostics;

namespace ActionRepeater.Input;

internal static class Recorder
{
    public static bool IsRecording { get; private set; }

    private static bool _isSubscribed;

    private static bool _shouldRecordMouseMovement;

    private static int _lastMouseMoveTickCount;
    private static int _lastNewActionTickCount;
    private static readonly TimeConsistencyChecker _wheelMsgTCC = new();

    private static void ReplaceLastAction(InputAction newAction)
    {
        // the caller of this func always checks if the action list is not empty
        if (ActionManager.Actions[^1] == ActionManager.ActionsExlKeyRepeat[^1])
        {
            ActionManager.ActionsExlKeyRepeat[^1] = newAction;
        }
        ActionManager.Actions[^1] = newAction;
    }

    private static InputAction? GetLastAction()
    {
        if (ActionManager.Actions.Count > 0) return ActionManager.Actions[^1];
        return null;
    }

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
            RegisterRawInput();
        }

        Reset();

        _shouldRecordMouseMovement = Options.Instance.CursorMovementMode != CursorMovementMode.None;

        if (_shouldRecordMouseMovement)
        {
            if (ActionManager.CursorPathStart is null)
            {
                ActionManager.CursorPathStart = new(Win32.PInvoke.Helpers.GetCursorPos(), 0);
                Debug.WriteLine($"set cursor start path pos to: {ActionManager.CursorPathStart.MovPoint}");
            }
            _lastMouseMoveTickCount = Environment.TickCount;
        }

        IsRecording = true;
    }

    public static void StopRecording()
    {
        if (!_isSubscribed)
        {
            return;
        }

        UnregisterRawInput();

        if (ActionManager.CursorPath.Count == 0) ActionManager.CursorPathStart = null;

        IsRecording = false;
    }

    public static void RegisterRawInput()
    {
        RAWINPUTDEVICE[] rid = new[]
        {
            new RAWINPUTDEVICE()
            {
                usUsagePage = UsagePage.GenericDesktopControl,
                usUsage = UsageId.Mouse,
                dwFlags = RawInputFlags.INPUTSINK,
                hwndTarget = App.MainWindow.Handle
            },
            new RAWINPUTDEVICE()
            {
                usUsagePage = UsagePage.GenericDesktopControl,
                usUsage = UsageId.Keyboard,
                dwFlags = RawInputFlags.INPUTSINK,
                hwndTarget = App.MainWindow.Handle
            }
        };

        if (!Win32.PInvoke.RegisterRawInputDevices(rid))
        {
            throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastPInvokeError());
        }

        _isSubscribed = true;
    }

    public static void UnregisterRawInput()
    {
        RAWINPUTDEVICE[] rid = new[]
        {
            new RAWINPUTDEVICE()
            {
                usUsagePage = UsagePage.GenericDesktopControl,
                usUsage = UsageId.Mouse,
                dwFlags = RawInputFlags.REMOVE,
                hwndTarget = IntPtr.Zero // hwndTarget must be IntPtr.Zero when RawInputFlags.REMOVE is set
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

    public static void OnInputMessage(Messaging.WindowMessageEventArgs e)
    {
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
                    //Debug.WriteLine($"{data.usFlags} - {data.lLastX}, {data.lLastY}");

                    //var pos = Win32.PInvoke.Helpers.GetCursorPos();
                    //Debug.WriteLine($"{data.usFlags} - {pos.x}, {pos.y}");

                    if (_shouldRecordMouseMovement)
                    {
                        int curTickCount = Environment.TickCount;
                        int ticksSinceLastMov = curTickCount - _lastMouseMoveTickCount;

                        ActionManager.CursorPath.Add(new(new Win32.POINT(data.lLastX, data.lLastY), ticksSinceLastMov));

                        _lastMouseMoveTickCount = curTickCount;
                    }
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
                                Win32.PInvoke.Helpers.GetCursorPos(), Options.Instance.UseCursorPosOnClicks));
                            break;
                        case RawMouseButtonState.LEFT_BUTTON_UP:
                            if (App.MainWindow.IsHoveringOverExcl) break;
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonUp, InputSimulator.MouseButton.Left,
                                Win32.PInvoke.Helpers.GetCursorPos(), Options.Instance.UseCursorPosOnClicks));
                            break;

                        case RawMouseButtonState.RIGHT_BUTTON_DOWN:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonDown, InputSimulator.MouseButton.Right,
                                Win32.PInvoke.Helpers.GetCursorPos(), Options.Instance.UseCursorPosOnClicks));
                            break;
                        case RawMouseButtonState.RIGHT_BUTTON_UP:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonUp, InputSimulator.MouseButton.Right,
                                Win32.PInvoke.Helpers.GetCursorPos(), Options.Instance.UseCursorPosOnClicks));
                            break;

                        case RawMouseButtonState.MIDDLE_BUTTON_DOWN:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonDown, InputSimulator.MouseButton.Middle,
                                Win32.PInvoke.Helpers.GetCursorPos(), Options.Instance.UseCursorPosOnClicks));
                            break;
                        case RawMouseButtonState.MIDDLE_BUTTON_UP:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonUp, InputSimulator.MouseButton.Middle,
                                Win32.PInvoke.Helpers.GetCursorPos(), Options.Instance.UseCursorPosOnClicks));
                            break;

                        case RawMouseButtonState.XBUTTON1_DOWN:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonDown, InputSimulator.MouseButton.X1,
                                Win32.PInvoke.Helpers.GetCursorPos(), Options.Instance.UseCursorPosOnClicks));
                            break;
                        case RawMouseButtonState.XBUTTON1_UP:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonUp, InputSimulator.MouseButton.X1,
                                Win32.PInvoke.Helpers.GetCursorPos(), Options.Instance.UseCursorPosOnClicks));
                            break;

                        case RawMouseButtonState.XBUTTON2_DOWN:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonDown, InputSimulator.MouseButton.X2,
                                Win32.PInvoke.Helpers.GetCursorPos(), Options.Instance.UseCursorPosOnClicks));
                            break;
                        case RawMouseButtonState.XBUTTON2_UP:
                            AddAction(new MouseButtonAction(MouseButtonAction.Type.MouseButtonUp, InputSimulator.MouseButton.X2,
                                Win32.PInvoke.Helpers.GetCursorPos(), Options.Instance.UseCursorPosOnClicks));
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
                    var actions = ActionManager.Actions;
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

    private static void AddAction(InputAction action)
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

        if (ticksSinceLastAction <= Options.Instance.MaxClickInterval)
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
            ActionManager.AddAction(new WaitAction(ticksSinceLastAction));
        }

        ActionManager.AddAction(action);
        App.MainWindow.ScrollActionList();

        _lastNewActionTickCount = curTickCount;
    }
}
