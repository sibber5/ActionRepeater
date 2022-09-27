using System;
using System.ComponentModel;
using ActionRepeater.Core.Input;
using ActionRepeater.Win32.Synch.Utilities;

namespace ActionRepeater.Core.Action;

public sealed class MouseWheelAction : WaitableInputAction, IEquatable<MouseWheelAction>
{
    public override string Name => IsHorizontal ? "Horizontal Mouse Wheel" : "Mouse Wheel";

    public override string Description => IsHorizontal
        ? ActionDescriptionTemplates.HorizontalWheelSteps(StepCount, Duration)
        : ActionDescriptionTemplates.WheelSteps(StepCount, Duration);

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

    private int _duration;
    public int Duration
    {
        get => _duration;
        set
        {
            if (_duration == value) return;

            _duration = value;
            OnDescriptionChanged();
        }
    }

    /// <param name="duration">The time it took/takes to scroll the wheel, in milliseconds/ticks.</param>
    public MouseWheelAction(bool isHorizontal, int stepCount, int duration = 0)
    {
        IsHorizontal = isHorizontal;
        _stepCount = stepCount;
        _duration = duration;
    }

    public override void PlayWait(HighResolutionWaiter waiter)
    {
        int absStepCount = Math.Abs(_stepCount);

        if (_duration == 0 || absStepCount == 1)
        {
            SendWheelEvent(_stepCount);
            return;
        }

        uint waitInterval = (uint)(_duration / (absStepCount - 1));
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

    /// <summary>
    /// Checks if the object's values are equal.<br/>
    /// Use equality operators (== and !=) to check if the references are equal or not.
    /// </summary>
    public bool Equals(MouseWheelAction? other) => other is not null
        && other.IsHorizontal == IsHorizontal
        && other.StepCount == _stepCount
        && other.Duration == _duration;

    /// <inheritdoc cref="Equals(MouseWheelAction)"/>
    public override bool Equals(object? obj) => Equals(obj as MouseWheelAction);

    public override int GetHashCode() => HashCode.Combine(IsHorizontal, _stepCount, _duration);
}
