using System.ComponentModel;
using System.Text.Json.Serialization;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Input;
using ActionRepeater.Win32;

namespace ActionRepeater.Core.Action;

public sealed class MouseButtonAction : InputAction
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

    [JsonIgnore]
    public override string Name => ActionType.ToString().AddSpacesBetweenWords();
    [JsonIgnore]
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
            MouseButtonActionType.MouseButtonDown  => _usePosition ? InputSimulator.SendMouseButtonDown(_button, _position)  : InputSimulator.SendMouseButtonDown(_button),
            MouseButtonActionType.MouseButtonUp    => _usePosition ? InputSimulator.SendMouseButtonUp(_button, _position)    : InputSimulator.SendMouseButtonUp(_button),
            MouseButtonActionType.MouseButtonClick => _usePosition ? InputSimulator.SendMouseButtonClick(_button, _position) : InputSimulator.SendMouseButtonClick(_button),
            _ => throw new InvalidEnumArgumentException("Invalid mouse button action.")
        };

        if (!success)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send mouse event ({ActionType}).");
            throw new Win32Exception(System.Runtime.InteropServices.Marshal.GetLastPInvokeError());
        }
    }
}

public enum MouseButtonActionType
{
    MouseButtonClick,
    MouseButtonDown,
    MouseButtonUp,
}
