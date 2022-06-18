using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActionRepeater.Core;

namespace ActionRepeater.UI.ViewModels;

public class CmdBarOptionsViewModel : ViewModelBase
{
    public int CursorMovementCBSelectedIdx
    {
        get => (int)Options.Instance.CursorMovementMode;
        set => Options.Instance.CursorMovementMode = (CursorMovementMode)value;
    }

    public bool UseCursorPosOnClicks
    {
        get => Options.Instance.UseCursorPosOnClicks;
        set => Options.Instance.UseCursorPosOnClicks = value;
    }

    public double MaxClickInterval
    {
        get => Options.Instance.MaxClickInterval;
        set => Options.Instance.MaxClickInterval = (int)Math.Round(value);
    }

    public bool SendKeyAutoRepeat
    {
        get => Options.Instance.SendKeyAutoRepeat;
        set => Options.Instance.SendKeyAutoRepeat = value;
    }

    public CmdBarOptionsViewModel()
    {
        Options.Instance.PropertyChanged += Options_PropertyChanged;
    }

    private void Options_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Options.Instance.CursorMovementMode):
                RaisePropertyChanged(nameof(CursorMovementCBSelectedIdx));
                break;

            default:
                RaisePropertyChanged(e.PropertyName!);
                break;
        }
    }
}
