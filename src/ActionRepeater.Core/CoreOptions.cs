using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ActionRepeater.Win32;

namespace ActionRepeater.Core;

public sealed class CoreOptions : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    private bool SetProperty<T>([NotNullIfNotNull(nameof(newValue))] ref T field, T newValue, [CallerMemberName] string propertyName = null!)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue)) return false;

        field = newValue;

        OnPropertyChanged(propertyName);

        return true;
    }

    private int _maxClickInterval = 120;
    public int MaxClickInterval
    {
        get => _maxClickInterval;
        set => SetProperty(ref _maxClickInterval, value);
    }

    private bool _sendKeyAutoRepeat = true;
    public bool SendKeyAutoRepeat
    {
        get => _sendKeyAutoRepeat;
        set => SetProperty(ref _sendKeyAutoRepeat, value);
    }

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
        set => SetProperty(ref _useCursorPosOnClicks, value);
    }

    private int _playRepeatCount = 1;
    public int PlayRepeatCount
    {
        get => _playRepeatCount;
        set => SetProperty(ref _playRepeatCount, value);
    }
}

public enum CursorMovementMode
{
    None = 0,
    Absolute,
    Relative
}
