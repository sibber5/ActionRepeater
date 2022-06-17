using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ActionRepeater.UI.Commands;

namespace ActionRepeater.UI.ViewModels;

public class CmdBarHomeViewModel : ViewModelBase
{
    public ICommand RecordCommand { get; }

    public CmdBarHomeViewModel()
    {
        RecordCommand = new RecordActionsCommand();
    }
}
