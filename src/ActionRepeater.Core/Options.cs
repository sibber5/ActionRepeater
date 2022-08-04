using System.ComponentModel;
using System.Runtime.CompilerServices;
using ActionRepeater.Core.Action;

namespace ActionRepeater.Core;

public class Options : INotifyPropertyChanged
{
    /// <summary>
    /// For deserialization only. will be private once https://github.com/dotnet/runtime/issues/31511 is implemented.
    /// </summary>
    public Options() { }
    private static Options? _instance;
    public static Options Instance => _instance ??= new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private CursorMovementMode _cursorMovementMode = CursorMovementMode.None;
    public CursorMovementMode CursorMovementMode
    {
        get => _cursorMovementMode;
        set
        {
            _cursorMovementMode = value;
            OnPropertyChanged();
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
            OnPropertyChanged();
        }
    }

    private int _maxClickInterval = 120;
    public int MaxClickInterval
    {
        get => _maxClickInterval;
        set
        {
            _maxClickInterval = value;
            OnPropertyChanged();
        }
    }

    private bool _sendKeyAutoRepeat = true;
    public bool SendKeyAutoRepeat
    {
        get => _sendKeyAutoRepeat;
        set
        {
            _sendKeyAutoRepeat = value;
            OnPropertyChanged();
        }
    }

    private int _playRepeatCount = 1;
    public int PlayRepeatCount
    {
        get => _playRepeatCount;
        set
        {
            _playRepeatCount = value;
            OnPropertyChanged();
        }
    }

    public static void Load(Options options)
    {
        _instance = options;
    }
}

public enum CursorMovementMode
{
    None = 0,
    Absolute,
    Relative
}
