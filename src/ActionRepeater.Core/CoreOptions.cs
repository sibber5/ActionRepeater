using System.ComponentModel;
using System.Runtime.CompilerServices;
using ActionRepeater.Win32;

namespace ActionRepeater.Core;

public sealed class CoreOptions : INotifyPropertyChanged
{
    /// <summary>
    /// For deserialization only. will be private once https://github.com/dotnet/runtime/issues/31511 is implemented.
    /// </summary>
    public CoreOptions() { }
    private static CoreOptions? _instance;
    public static CoreOptions Instance => _instance ??= new();

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private CursorMovementMode _cursorMovementMode = CursorMovementMode.None;
    public CursorMovementMode CursorMovementMode
    {
        get => _cursorMovementMode;
        set
        {
            if (value == CursorMovementMode.Absolute)
            {
                SystemInformation.RefreshMonitorSettings();
                SystemInformation.RefreshMouseSettings();
            }

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

    public static void Load(CoreOptions options)
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
