using System;
using System.ComponentModel;
using ActionRepeater.Core.Extentions;
using static ActionRepeater.Core.Input.InputSimulator;
using POINT = ActionRepeater.Win32.POINT;

namespace ActionRepeater.Core.Action;

public sealed class MouseButtonAction : InputAction, IEquatable<MouseButtonAction>
{
    private MouseButtonActionType _actionType;
    public MouseButtonActionType ActionType
    {
        get => _actionType;
        set
        {
            if (_actionType == value) return;

            _actionType = value;
            OnNameChanged();
        }
    }

    public override string Name => ActionType.ToString().AddSpacesBetweenWords();

    public override string Description => UsePosition ? ActionDescriptionTemplates.ButtonPoint(Button, Position) : ActionDescriptionTemplates.Button(Button);

    private MouseButton _button;
    public MouseButton Button
    {
        get => _button;
        set
        {
            if (_button == value) return;

            _button = value;
            OnDescriptionChanged();
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
            OnDescriptionChanged();
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
            OnDescriptionChanged();
        }
    }

    public MouseButtonAction(MouseButtonActionType type, MouseButton button, POINT position, bool usePosition = true)
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
            MouseButtonActionType.MouseButtonDown  => _usePosition ? SendMouseButtonDown(_button, _position)  : SendMouseButtonDown(_button),
            MouseButtonActionType.MouseButtonUp    => _usePosition ? SendMouseButtonUp(_button, _position)    : SendMouseButtonUp(_button),
            MouseButtonActionType.MouseButtonClick => _usePosition ? SendMouseButtonClick(_button, _position) : SendMouseButtonClick(_button),
            _ => throw new InvalidEnumArgumentException("Invalid mouse button action.")
        };

        if (!success)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send mouse event ({ActionType}).");
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

public enum MouseButtonActionType
{
    MouseButtonClick = 1,
    MouseButtonDown,
    MouseButtonUp,
}
