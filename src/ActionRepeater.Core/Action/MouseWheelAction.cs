using System;
using System.ComponentModel;
using ActionRepeater.Core.Input;

namespace ActionRepeater.Core.Action;

public sealed class MouseWheelAction : InputAction, IEquatable<MouseWheelAction>
{
    public override string Name => IsHorizontal ? "Horizontal Mouse Wheel" : "Mouse Wheel";

    public override string Description
    {
        get
        {
            if (Duration == 0)
            {
                return IsHorizontal
                    ? ActionDescriptionTemplates.HorizontalWheelSteps(StepCount)
                    : ActionDescriptionTemplates.WheelSteps(StepCount);
            }

            return IsHorizontal
                ? ActionDescriptionTemplates.HorizontalWheelSteps(StepCount, Duration)
                : ActionDescriptionTemplates.WheelSteps(StepCount, Duration);
        }
    }

    public bool IsHorizontal { get; }

    private int _stepCount;
    public int StepCount
    {
        get => _stepCount;
        set
        {
            if (_stepCount == value) return;

            _stepCount = value;
            RaisePropertyChanged(nameof(Description));
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
            RaisePropertyChanged(nameof(Description));
        }
    }

    /// <param name="duration">The time it took/takes to scroll the wheel, in milliseconds/ticks.</param>
    public MouseWheelAction(bool isHorizontal, int stepCount, int duration = 0)
    {
        IsHorizontal = isHorizontal;
        _stepCount = stepCount;
        _duration = duration;
    }

    public override void Play()
    {
        int absStepCount = Math.Abs(_stepCount);

        if (_duration == 0 || absStepCount == 1)
        {
            SendWheelEvent(_stepCount);
            return;
        }

        int waitInterval = _duration / (absStepCount - 1);
        int step = Math.Sign(_stepCount);

        SendWheelEvent(step);
        for (int i = 1; i < absStepCount; ++i)
        {
            System.Threading.Thread.Sleep(waitInterval);
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
