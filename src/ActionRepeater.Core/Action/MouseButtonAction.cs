using System;
using System.ComponentModel;
using ActionRepeater.Core.Extentions;
using static ActionRepeater.Core.Input.InputSimulator;
using POINT = ActionRepeater.Win32.POINT;

namespace ActionRepeater.Core.Action;

public sealed class MouseButtonAction : InputAction, IEquatable<MouseButtonAction>
{
    public enum @Type
    {
        MouseButtonClick = 1,
        MouseButtonDown,
        MouseButtonUp,
    }

    public @Type ActionType { get; }

    private string? _name;
    public override string Name
    {
        get
        {
            if (_name is null)
            {
                _name = ActionType.ToString().AddSpacesBetweenWords();
            }

            return _name;
        }
    }

    private string? _description;
    public override string Description
    {
        get
        {
            if (_description is null)
            {
                UpdateDescription();
            }

            return _description!;
        }
    }
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
            RaisePropertyChanged(nameof(Description));
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
            RaisePropertyChanged(nameof(Description));
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
            RaisePropertyChanged(nameof(Description));
        }
    }

    public MouseButtonAction(@Type type, MouseButton button, POINT position, bool usePosition = true)
    {
        ActionType = type;
        _button = button;
        _position = position;
        _usePosition = usePosition;
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

    public override int GetHashCode() => HashCode.Combine(ActionType, _button, _position, _usePosition);
}
