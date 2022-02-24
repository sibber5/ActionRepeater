using Math = System.Math;
using System.ComponentModel;
using ActionRepeater.Input;

namespace ActionRepeater.Action;

public sealed class MouseWheelAction : InputAction, System.IEquatable<MouseWheelAction>
{
    public override string Name { get => IsHorizontal ? "Horizontal Mouse Wheel" : "Mouse Wheel"; }

    private string _description;
    public override string Description => _description;

    public bool IsHorizontal { get; }

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
                _description = IsHorizontal
                    ? ActionDescriptionTemplates.HorizontalWheelSteps(value)
                    : ActionDescriptionTemplates.WheelSteps(value);
            }
            else
            {
                _description = IsHorizontal
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
                _description = IsHorizontal
                    ? ActionDescriptionTemplates.HorizontalWheelSteps(_stepCount)
                    : ActionDescriptionTemplates.WheelSteps(_stepCount);
            }
            else
            {
                _description = IsHorizontal
                    ? ActionDescriptionTemplates.HorizontalWheelSteps(_stepCount, value)
                    : ActionDescriptionTemplates.WheelSteps(_stepCount, value);
            }
            NotifyPropertyChanged(nameof(Description));
        }
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

    public override InputAction Clone() => new MouseWheelAction(_description, IsHorizontal, _stepCount, _duration);

    private MouseWheelAction(string description, bool isHorizontal, int stepCount, int duration = 0)
    {
        _description = description;
        IsHorizontal = isHorizontal;
        _stepCount = stepCount;
        _duration = duration;
    }

    /// <param name="duration">The time it took/takes to scroll the wheel, in milliseconds/ticks.</param>
    public MouseWheelAction(bool isHorizontal, int stepCount, int duration = 0)
    {
        IsHorizontal = isHorizontal;
        _stepCount = stepCount;
        _duration = duration;
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

    /// <summary>
    /// Checks if the object's values are equal.<br/>
    /// Use equality operators (== and !=) to check if the references are equal or not.
    /// </summary>
    public bool Equals(MouseWheelAction other) => other is not null
        && other.IsHorizontal == IsHorizontal
        && other.StepCount == _stepCount
        && other.Duration == _duration;

    /// <inheritdoc cref="Equals(MouseWheelAction)"/>
    public override bool Equals(object obj) => Equals(obj as MouseWheelAction);

    public override int GetHashCode() => System.HashCode.Combine(IsHorizontal, _stepCount, _duration);

    public static async System.Threading.Tasks.Task<MouseWheelAction> CreateActionFromXmlAsync(System.Xml.XmlReader reader)
    {
        await reader.ReadAsync(); // move to Start Element IsHorizontal

        ThrowIfInvalidName(nameof(IsHorizontal));
        bool isHorizontal = reader.ReadElementContentAsBoolean(); // this moves to start of next element

        ThrowIfInvalidName(nameof(StepCount));
        int stepCount = reader.ReadElementContentAsInt();

        ThrowIfInvalidName(nameof(Duration));
        int duration = reader.ReadElementContentAsInt();

        return new MouseWheelAction(isHorizontal, stepCount, duration);

        void ThrowIfInvalidName(string name)
        {
            if (!reader.Name.Equals(name, System.StringComparison.Ordinal))
            {
                throw new System.FormatException($"Unexpected element \"{reader.Name}\". Expected \"{name}\".");
            }
        }
    }

    public override void WriteXml(System.Xml.XmlWriter writer)
    {
        writer.WriteAttributeString("Type", nameof(MouseWheelAction));

        writer.WriteElementString(nameof(IsHorizontal), IsHorizontal.ToString(System.Globalization.CultureInfo.InvariantCulture).ToLowerInvariant());
        writer.WriteElementString(nameof(StepCount), _stepCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
        writer.WriteElementString(nameof(Duration), _duration.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}
