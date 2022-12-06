using System;
using System.Diagnostics;
using System.Linq;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Utilities;
using ActionRepeater.Win32.Input;
using ActionRepeater.Win32.WindowsAndMessages.Utilities;

namespace ActionRepeater.Core.Input;

public sealed class Recorder
{
    private const int MinWaitDuration = 10; // in milliseconds

    public event EventHandler<bool>? IsRecordingChanged;
    public event EventHandler<InputAction>? ActionAdded;

    public bool IsRecording { get; private set; }

    public bool IsSubscribed { get; private set; }

    /// <summary>
    /// Function that returns true if the mouse click shouldnt be recorded.
    /// </summary>
    public Func<bool>? ShouldRecordMouseClick;

    private bool _shouldRecordMouseMovement;

    private readonly StopwatchSlim _mouseStopwatch = new();
    private readonly StopwatchSlim _actionStopwatch = new();
    private readonly TimeConsistencyChecker _wheelMsgTCC = new();

    private readonly ActionCollection _actionCollection;

    public Recorder(ActionCollection actionCollection)
    {
        _actionCollection = actionCollection;

        // this *could* cause a memory leak *if* the action collection should live longer than this recorder instance,
        // *but* this will not happen with the current usage of these, as both are registered as a singleton.
        // if for some reason in the future that changed this class would implement IDisposable and unsubscribe from the event in Dispose.
        _actionCollection.ActionsCountChanged += (_, _) =>
        {
            if (_actionCollection.Actions.Count == 0)
            {
                Restart();
            }
        };
    }

    private InputAction? GetLastAction()
    {
        if (_actionCollection.Actions.Count > 0) return _actionCollection.Actions[^1];
        return null;
    }

    private void ReplaceLastAction(InputAction action)
    {
        // the caller of this func always checks if the action list is not empty
        if (_actionCollection.Actions[^1] == _actionCollection.ActionsExlKeyRepeat[^1])
        {
            ((ObservableCollectionEx<InputAction>)_actionCollection.ActionsExlKeyRepeat)[^1] = action;
        }
        ((ObservableCollectionEx<InputAction>)_actionCollection.Actions)[^1] = action;
    }

    public void Restart()
    {
        _wheelMsgTCC.Reset();
        _actionStopwatch.Restart();
    }

    public void StartRecording()
    {
        if (!IsSubscribed)
        {
            throw new InvalidOperationException($"Not currently registered for raw input. Call {nameof(Recorder)}.{nameof(Recorder.RegisterRawInput)}(IntPtr) to register.");
        }

        Restart();

        _shouldRecordMouseMovement = CoreOptions.Instance.CursorMovementMode != CursorMovementMode.None;

        if (_shouldRecordMouseMovement)
        {
            if (_actionCollection.CursorPathStart is null)
            {
                Debug.Assert(_actionCollection.CursorPath.Count == 0, $"{nameof(_actionCollection.CursorPath)} is not empty.");

                _actionCollection.CursorPathStart = new(Win32.PInvoke.Helpers.GetCursorPos(), 0);
                Debug.WriteLine($"set cursor start path pos to: {_actionCollection.CursorPathStart.Delta}");
            }

            _mouseStopwatch.Restart();
        }

        IsRecording = true;
        IsRecordingChanged?.Invoke(this, true);
    }

    public void StopRecording()
    {
        if (!IsSubscribed) return;

        UnregisterRawInput();

        if (_actionCollection.CursorPath.Count == 0) _actionCollection.CursorPathStart = null;

        IsRecording = false;
        IsRecordingChanged?.Invoke(this, false);
    }

    public void RegisterRawInput(IntPtr targetWindowHandle)
    {
        var rid = new RAWINPUTDEVICE[]
        {
            new()
            {
                usUsagePage = UsagePage.GenericDesktopControl,
                usUsage = UsageId.Mouse,
                dwFlags = RawInputFlags.INPUTSINK,
                hwndTarget = targetWindowHandle
            },
            new()
            {
                usUsagePage = UsagePage.GenericDesktopControl,
                usUsage = UsageId.Keyboard,
                dwFlags = RawInputFlags.INPUTSINK,
                hwndTarget = targetWindowHandle
            }
        };

        if (!Win32.PInvoke.RegisterRawInputDevices(rid))
        {
            throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastPInvokeError());
        }

        IsSubscribed = true;
    }

    public void UnregisterRawInput()
    {
        var inputDevices = new RAWINPUTDEVICE[]
        {
            new()
            {
                usUsagePage = UsagePage.GenericDesktopControl,
                usUsage = UsageId.Mouse,
                dwFlags = RawInputFlags.REMOVE,
                hwndTarget = IntPtr.Zero // hwndTarget must be IntPtr.Zero when RawInputFlags.REMOVE is set
            },
            new()
            {
                usUsagePage = UsagePage.GenericDesktopControl,
                usUsage = UsageId.Keyboard,
                dwFlags = RawInputFlags.REMOVE,
                hwndTarget = IntPtr.Zero
            }
        };

        if (!Win32.PInvoke.RegisterRawInputDevices(inputDevices))
        {
            throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastPInvokeError());
        }

        IsSubscribed = false;
    }

    public void OnInputMessage(WindowMessageEventArgs e)
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
                    if (!_shouldRecordMouseMovement) break;

                    if (data.usFlags.HasFlag(RawMouseState.MOVE_ABSOLUTE))
                    {
                        //bool isVirtualDesktop = data.usFlags.HasFlag(RawMouseState.VIRTUAL_DESKTOP);
                        throw new NotSupportedException($"Mouse movement mode not supported. ({data.usFlags})");
                    }
                    else
                    {
                        OnMouseMove(data.lLastX, data.lLastY);
                    }
                }
                else // button/wheel event
                {
                    var buttonInfo = data.rawButtonData.buttonInfo;
                    switch (buttonInfo.usButtonFlags)
                    {
                        case RawMouseButtonState.HWHEEL:
                        case RawMouseButtonState.WHEEL:
                            OnMouseWheelMessage(buttonInfo);
                            break;

                        default:
                            OnMouseButtonMessage(buttonInfo.usButtonFlags);
                            break;
                    }
                }

                break;

            case RawInputType.Keyboard:
                VirtualKey key = inputData.data.keyboard.VKey;
                if (key == VirtualKey.NO_KEY || (ushort)key == 255) break;

                OnKeyboardMessage(key, inputData.data.keyboard.Flags);
                break;
        }

        if (inputCode == 0)
        {
            e.Handled = true;
            e.Result = Win32.PInvoke.DefWindowProc(e.Message.hwnd, e.Message.message, e.Message.wParam, e.Message.lParam);
        }
    }

    private void OnMouseMove(int deltaX, int deltaY)
    {
        int timeSinceLastMov = (int)_mouseStopwatch.RestartAndGetElapsedMS();
        _actionCollection.CursorPath.Add(new(new Win32.POINT(deltaX, deltaY), timeSinceLastMov));
    }

    private void OnMouseWheelMessage(RAWMOUSE.RawButtonData.RawButtonInfo buttonInfo)
    {
        int wheelStepCount = unchecked((short)buttonInfo.usButtonData) / 120;

        if (_wheelMsgTCC.UpdateAndCheckIfConsistent() && GetLastAction() is MouseWheelAction lastMWAction)
        {
            int prevWheelStepCount = lastMWAction.StepCount;

            if ((wheelStepCount < 0) == (prevWheelStepCount < 0))
            {
                lastMWAction.StepCount = prevWheelStepCount + wheelStepCount;
                lastMWAction.Duration = _wheelMsgTCC.TickDeltasTotal;
                return;
            }
        }

        AddAction(new MouseWheelAction(buttonInfo.usButtonFlags == RawMouseButtonState.HWHEEL, wheelStepCount));
    }

    private void OnMouseButtonMessage(RawMouseButtonState buttonState)
    {
        var (button, type) = buttonState switch
        {
            RawMouseButtonState.LEFT_BUTTON_DOWN   => (MouseButton.Left, MouseButtonActionType.MouseButtonDown),
            RawMouseButtonState.LEFT_BUTTON_UP     => (MouseButton.Left, MouseButtonActionType.MouseButtonUp),

            RawMouseButtonState.RIGHT_BUTTON_DOWN  => (MouseButton.Right, MouseButtonActionType.MouseButtonDown),
            RawMouseButtonState.RIGHT_BUTTON_UP    => (MouseButton.Right, MouseButtonActionType.MouseButtonUp),

            RawMouseButtonState.MIDDLE_BUTTON_DOWN => (MouseButton.Middle, MouseButtonActionType.MouseButtonDown),
            RawMouseButtonState.MIDDLE_BUTTON_UP   => (MouseButton.Middle, MouseButtonActionType.MouseButtonUp),

            RawMouseButtonState.XBUTTON1_DOWN      => (MouseButton.X1, MouseButtonActionType.MouseButtonDown),
            RawMouseButtonState.XBUTTON1_UP        => (MouseButton.X1, MouseButtonActionType.MouseButtonUp),

            RawMouseButtonState.XBUTTON2_DOWN      => (MouseButton.X2, MouseButtonActionType.MouseButtonDown),
            RawMouseButtonState.XBUTTON2_UP        => (MouseButton.X2, MouseButtonActionType.MouseButtonUp),

            _ => throw new NotSupportedException($"{buttonState} not supported.")
        };

        if (button == MouseButton.Left && ShouldRecordMouseClick?.Invoke() == true) return;

        AddAction(new MouseButtonAction(type, button, Win32.PInvoke.Helpers.GetCursorPos(), CoreOptions.Instance.UseCursorPosOnClicks));
    }

    private void OnKeyboardMessage(VirtualKey key, RawInputKeyFlags keyFlags)
    {
        if (keyFlags.HasFlag(RawInputKeyFlags.BREAK)) // key up
        {
            AddAction(new KeyAction(KeyActionType.KeyUp, key));
        }
        else //if (keyFlags.HasFlag(RawInputKeyFlags.MAKE))
        {
            AddAction(new KeyAction(KeyActionType.KeyDown, key, IsAutoRepeat()));
        }

        bool IsAutoRepeat()
        {
            var actions = _actionCollection.Actions;

            for (int i = actions.Count - 1; i > -1; --i)
            {
                if (actions[i] is KeyAction keyActionI && keyActionI.Key == key)
                {
                    return keyActionI.ActionType == KeyActionType.KeyDown;
                }
            }

            return false;
        }
    }

    private void AddAction(InputAction action)
    {
        if (action is not MouseWheelAction)
        {
            _wheelMsgTCC.Reset();
        }

        int elapsedMS = (int)_actionStopwatch.RestartAndGetElapsedMS();

        if (elapsedMS <= CoreOptions.Instance.MaxClickInterval)
        {
            if (CheckAndReplaceWithClickAction(action))
            {
                return;
            }
        }

        if (elapsedMS >= MinWaitDuration)
        {
            _actionCollection.Add(new WaitAction(elapsedMS));
        }

        _actionCollection.Add(action);

        ActionAdded?.Invoke(this, action);
    }

    /// <returns>true if a click action was added, otherwise false.</returns>
    private bool CheckAndReplaceWithClickAction(InputAction currentActionToAdd)
    {
        if (currentActionToAdd is KeyAction curKeyAction
            && curKeyAction.ActionType == KeyActionType.KeyUp
            && GetLastAction() is KeyAction lastKeyAction
            && lastKeyAction.ActionType == KeyActionType.KeyDown
            && !lastKeyAction.IsAutoRepeat
            && lastKeyAction.Key == curKeyAction.Key)
        {
            ReplaceLastAction(new KeyAction(KeyActionType.KeyPress, curKeyAction.Key));
            return true;
        }

        if (currentActionToAdd is MouseButtonAction curMBAction
            && curMBAction.ActionType == MouseButtonActionType.MouseButtonUp
            && GetLastAction() is MouseButtonAction lastMBAction
            && lastMBAction.ActionType == MouseButtonActionType.MouseButtonDown
            && lastMBAction.Button == curMBAction.Button
            && lastMBAction.Position == curMBAction.Position)
        {
            ReplaceLastAction(new MouseButtonAction(MouseButtonActionType.MouseButtonClick, curMBAction.Button, curMBAction.Position, curMBAction.UsePosition));
            return true;
        }

        return false;
    }
}
