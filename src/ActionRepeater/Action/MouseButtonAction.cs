using System.ComponentModel;
using static ActionRepeater.Input.InputSimulator;
using POINT = ActionRepeater.Win32.POINT;

namespace ActionRepeater.Action;

internal sealed class MouseButtonAction : IInputAction
{
    public enum @Type
    {
        MouseButtonDown,
        MouseButtonUp,
        MouseButtonClick
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public @Type ActionType { get; }

    public string Name
    {
        get
        {
            System.Text.StringBuilder sb = new();
            string name = ActionType.ToString();
            for (int i = 1; i < name.Length; ++i)
            {
                sb.Append(name[i - 1]);
                if (char.IsUpper(name[i]))
                {
                    sb.Append(' ');
                }
            }
            sb.Append(name[^1]);
            return sb.ToString();
        }
    }

    private string _description;
    public string Description => _description;
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

    public void Play()
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

    public IInputAction Clone() => new MouseButtonAction(ActionType, _button, _position);

    public MouseButtonAction(@Type type, MouseButton button, POINT position, bool usePosition = true)
    {
        ActionType = type;
        _button = button;
        _position = position;
        _usePosition = usePosition;
        UpdateDescription();
    }
}
