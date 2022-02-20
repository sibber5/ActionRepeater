using Math = System.Math;
using System.ComponentModel;
using ActionRepeater.Input;

namespace ActionRepeater.Action;

internal sealed class MouseWheelAction : IInputAction
{
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private readonly string _name;
    public string Name => _name;

    private string _description;
    public string Description => _description;

    private readonly bool _isHorizontal;

    private int _stepCount;
    public int StepCount
    {
        get => _stepCount;
        set
        {
            if (_stepCount == value) return;

            _stepCount = value;
            if (_duration == 0)
            {
                _description = _isHorizontal
                    ? ActionDescriptionTemplates.HorizontalWheelSteps(value)
                    : ActionDescriptionTemplates.WheelSteps(value);
            }
            else
            {
                _description = _isHorizontal
                    ? ActionDescriptionTemplates.HorizontalWheelSteps(value, _duration)
                    : ActionDescriptionTemplates.WheelSteps(value, _duration);
            }
            NotifyPropertyChanged(nameof(Description));
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
            if (value == 0)
            {
                _description = _isHorizontal
                    ? ActionDescriptionTemplates.HorizontalWheelSteps(_stepCount)
                    : ActionDescriptionTemplates.WheelSteps(_stepCount);
            }
            else
            {
                _description = _isHorizontal
                    ? ActionDescriptionTemplates.HorizontalWheelSteps(_stepCount, value)
                    : ActionDescriptionTemplates.WheelSteps(_stepCount, value);
            }
            NotifyPropertyChanged(nameof(Description));
        }
    }

    public void Play()
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
            if (!InputSimulator.MoveMouseWheel(clicks, _isHorizontal))
            {
                System.Diagnostics.Debug.WriteLine("Failed to send mouse wheel event.");
                throw new Win32Exception(System.Runtime.InteropServices.Marshal.GetLastPInvokeError());
            }
        }
    }

    public IInputAction Clone() => new MouseWheelAction(_name, _description, _isHorizontal, _stepCount, _duration);

    private MouseWheelAction(string name, string description, bool isHorizontal, int stepCount, int duration = 0)
    {
        _name = name;
        _description = description;
        _isHorizontal = isHorizontal;
        _stepCount = stepCount;
        _duration = duration;
    }

#pragma warning disable RCS1139 // Add summary element to documentation comment.
    /// <param name="duration">The time it took/takes to scroll the wheel, in milliseconds/ticks.</param>
    public MouseWheelAction(bool isHorizontal, int stepCount, int duration = 0)
    {
        _isHorizontal = isHorizontal;
        _stepCount = stepCount;
        _duration = duration;
        if (isHorizontal)
        {
            _name = "Horizontal Mouse Wheel";
        }
        else
        {
            _name = "Mouse Wheel";
        }
        if (_duration == 0)
        {
            _description = isHorizontal
                ? ActionDescriptionTemplates.HorizontalWheelSteps(stepCount)
                : ActionDescriptionTemplates.WheelSteps(stepCount);
        }
        else
        {
            _description = isHorizontal
                ? ActionDescriptionTemplates.HorizontalWheelSteps(stepCount, duration)
                : ActionDescriptionTemplates.WheelSteps(stepCount, duration);
        }
    }
}
