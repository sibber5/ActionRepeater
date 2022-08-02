using CommunityToolkit.Mvvm.ComponentModel;
using ActionRepeater.Core;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using ActionRepeater.UI.Services;
using System.Threading.Tasks;

namespace ActionRepeater.UI.ViewModels;

public partial class CmdBarOptionsViewModel : ObservableObject
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

    public int MaxClickInterval
    {
        get => Options.Instance.MaxClickInterval;
        set => Options.Instance.MaxClickInterval = value;
    }

    public bool SendKeyAutoRepeat
    {
        get => Options.Instance.SendKeyAutoRepeat;
        set => Options.Instance.SendKeyAutoRepeat = value;
    }

    public bool IsDarkModeEnabled => App.Current.RequestedTheme == Microsoft.UI.Xaml.ApplicationTheme.Dark;

    private readonly ContentDialogService _contentDialogService;

    public CmdBarOptionsViewModel(ContentDialogService contentDialogService)
    {
        _contentDialogService = contentDialogService;

        Options.Instance.PropertyChanged += Options_PropertyChanged;
    }

    private void Options_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Options.Instance.CursorMovementMode):
                OnPropertyChanged(nameof(CursorMovementCBSelectedIdx));
                break;

            default:
                OnPropertyChanged(e);
                break;
        }
    }

    [RelayCommand]
    public async Task ToggleDarkMode()
    {
        // theme is still the same but the toggle button has toggled, so notify to display correct toggle value
        OnPropertyChanged(nameof(IsDarkModeEnabled));

        await _contentDialogService.ShowYesNoMessageDialog("Restart required to change theme", "Restart?",
            onYesClick: static () =>
            {
                bool isDarkMode = App.Current.RequestedTheme == Microsoft.UI.Xaml.ApplicationTheme.Dark;

                string path = Path.ChangeExtension(System.Reflection.Assembly.GetEntryAssembly()!.Location, ".exe");
                System.Diagnostics.Process.Start(path, $"--theme={(isDarkMode ? "light" : "dark")}");
                Microsoft.UI.Xaml.Application.Current.Exit();

                //Microsoft.Windows.AppLifecycle.AppInstance.Restart($"--theme={(isDarkMode ? "light" : "dark")}"); // throws FileNotFoundException
            });
    }
}
