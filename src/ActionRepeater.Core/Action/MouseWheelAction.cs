using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using ActionRepeater.Core.Input;
using ActionRepeater.Win32.Synch.Utilities;

namespace ActionRepeater.Core.Action;

public sealed class MouseWheelAction : WaitableInputAction
{
    [JsonIgnore]
    public override string Name => IsHorizontal ? "Horizontal Mouse Wheel" : "Mouse Wheel";
    [JsonIgnore]
    public override string Description => IsHorizontal
        ? ActionDescriptionTemplates.HorizontalWheelSteps(StepCount, DurationMS)
        : ActionDescriptionTemplates.WheelSteps(StepCount, DurationMS);

    private bool _isHorizontal;
    public bool IsHorizontal
    {
        get => _isHorizontal;
        set
        {
            if (_isHorizontal == value) return;

            _isHorizontal = value;
            OnNameChanged();
        }
    }

    private int _stepCount;
    public int StepCount
    {
        get => _stepCount;
        set
        {
            if (_stepCount == value) return;

            _stepCount = value;
            OnDescriptionChanged();
        }
    }

    private int _durationMS;
    public int DurationMS
    {
        get => _durationMS;
        set
        {
            if (_durationMS == value) return;

            _durationMS = value;
            OnDescriptionChanged();
        }
    }

    /// <param name="durationMS">The time it took/takes to scroll the wheel, in milliseconds/ticks.</param>
    public MouseWheelAction(bool isHorizontal, int stepCount, int durationMS = 0)
    {
        IsHorizontal = isHorizontal;
        _stepCount = stepCount;
        _durationMS = durationMS;
    }

    public override void PlayWait(HighResolutionWaiter waiter)
    {
        int absStepCount = Math.Abs(_stepCount);

        if (_durationMS == 0 || absStepCount == 1)
        {
            SendWheelEvent(_stepCount);
            return;
        }

        uint waitInterval = (uint)(_durationMS / (absStepCount - 1));
        int step = Math.Sign(_stepCount);

        SendWheelEvent(step);
        for (int i = 1; i < absStepCount; ++i)
        {
            waiter.Wait(waitInterval);
            SendWheelEvent(step);
        }

        void SendWheelEvent(int clicks)
        {
            if (!InputSimulator.MoveMouseWheel(clicks, IsHorizontal))
            {
                System.Diagnostics.Debug.WriteLine("Failed to send mouse wheel event.");
                throw new Win32Exception(System.Runtime.InteropServices.Marshal.GetLastPInvokeError());
            }
        }
    }
}
