using System.ComponentModel;
using System.Runtime.CompilerServices;
using ActionRepeater.Core.Action;

namespace ActionRepeater.Core;

public class Options : INotifyPropertyChanged
{
    private Options() { }
    public static Options Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private CursorMovementMode _cursorMovementMode = CursorMovementMode.None;
    public CursorMovementMode CursorMovementMode
    {
        get => _cursorMovementMode;
        set
        {
            _cursorMovementMode = value;
            NotifyPropertyChanged();
        }
    }

    private bool _useCursorPosOnClicks = true;
    public bool UseCursorPosOnClicks
    {
        get => _useCursorPosOnClicks;
        set
        {
            if (_useCursorPosOnClicks == value) return;

            foreach (InputAction action in Input.ActionManager.Actions)
            {
                if (action is MouseButtonAction mbAction)
                {
                    mbAction.UsePosition = value;
                }
            }
            _useCursorPosOnClicks = value;
            NotifyPropertyChanged();
        }
    }

    private int _maxClickInterval = 120;
    public int MaxClickInterval
    {
        get => _maxClickInterval;
        set
        {
            _maxClickInterval = value;
            NotifyPropertyChanged();
        }
    }

    private bool _sendKeyAutoRepeat = true;
    public bool SendKeyAutoRepeat
    {
        get => _sendKeyAutoRepeat;
        set
        {
            _sendKeyAutoRepeat = value;
            NotifyPropertyChanged();
        }
    }
}

public enum CursorMovementMode
{
    None = 0,
    Absolute,
    Relative
}
