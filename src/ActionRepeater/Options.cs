using System.ComponentModel;
using System.Runtime.CompilerServices;
using ActionRepeater.Action;

namespace ActionRepeater;

public class Options : INotifyPropertyChanged
{
    private Options() { }
    public static Options Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public CursorMovementMode CursorMovementMode { get; internal set; } = CursorMovementMode.None;

    private bool _useCursorPosOnClicks = true;
    public bool UseCursorPosOnClicks
    {
        get => _useCursorPosOnClicks;
        internal set
        {
            if (_useCursorPosOnClicks == value) return;

            foreach (InputAction action in ActionRepeater.Input.ActionManager.Actions)
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

    public int MaxClickInterval { get; internal set; } = 120;

    private bool _sendKeyAutoRepeat = true;
    public bool SendKeyAutoRepeat
    {
        get => _sendKeyAutoRepeat;
        internal set
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
