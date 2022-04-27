using System.ComponentModel;
using ActionRepeater.Extentions;
using static ActionRepeater.Input.InputSimulator;
using POINT = ActionRepeater.Win32.POINT;

namespace ActionRepeater.Action;

public sealed class MouseButtonAction : InputAction, System.IEquatable<MouseButtonAction>
{
    public enum @Type
    {
        MouseButtonDown,
        MouseButtonUp,
        MouseButtonClick
    }

    public @Type ActionType { get; }

    public override string Name { get; }

    private string _description = default!;
    public override string Description { get => _description; }
    private void UpdateDescription()
    {
        _description = _usePosition
            ? ActionDescriptionTemplates.ButtonPoint(_button, _position)
            : ActionDescriptionTemplates.Button(_button);
    }

    private MouseButton _button;
    public MouseButton Button
    {
        get => _button;
        set
        {
            if (_button == value) return;

            _button = value;
            UpdateDescription();
            NotifyPropertyChanged(nameof(Description));
        }
    }

    private POINT _position;
    public POINT Position
    {
        get => _position;
        set
        {
            if (_position == value) return;

            _position = value;
            UpdateDescription();
            NotifyPropertyChanged(nameof(Description));
        }
    }

    private bool _usePosition;
    public bool UsePosition
    {
        get => _usePosition;
        set
        {
            if (_usePosition == value) return;

            _usePosition = value;
            UpdateDescription();
            NotifyPropertyChanged(nameof(Description));
        }
    }

    public override void Play()
    {
        bool success = ActionType switch
        {
            @Type.MouseButtonDown  => _usePosition ? SendMouseButtonDown(_button, _position)  : SendMouseButtonDown(_button),
            @Type.MouseButtonUp    => _usePosition ? SendMouseButtonUp(_button, _position)    : SendMouseButtonUp(_button),
            @Type.MouseButtonClick => _usePosition ? SendMouseButtonClick(_button, _position) : SendMouseButtonClick(_button),
            _ => throw new InvalidEnumArgumentException("Invalid mouse button action.")
        };

        if (!success)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send mosue event ({ActionType}).");
            throw new Win32Exception(System.Runtime.InteropServices.Marshal.GetLastPInvokeError());
        }
    }

    public override InputAction Clone() => new MouseButtonAction(ActionType, _button, _position);

    public MouseButtonAction(@Type type, MouseButton button, POINT position, bool usePosition = true)
    {
        ActionType = type;
        _button = button;
        _position = position;
        _usePosition = usePosition;
        Name = type.ToString().AddSpacesBetweenWords();
        UpdateDescription();
    }

    /// <summary>
    /// Checks if the object's values are equal.<br/>
    /// Use equality operators (== and !=) to check if the references are equal or not.
    /// </summary>
    public bool Equals(MouseButtonAction? other) => other is not null
        && other.ActionType == ActionType
        && other.Button == _button
        && other.Position == _position
        && other.UsePosition == _usePosition;

    /// <inheritdoc cref="Equals(MouseButtonAction)"/>
    public override bool Equals(object? obj) => Equals(obj as MouseButtonAction);

    public override int GetHashCode() => System.HashCode.Combine(ActionType, _button, _position, _usePosition);

    private MouseButtonAction() { Name = ActionType.ToString().AddSpacesBetweenWords(); }
}
