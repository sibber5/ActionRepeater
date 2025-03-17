using ActionRepeater.Core.Action;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.ViewModels;

public sealed partial class EditTextTypeActionViewModel : ObservableObject
{
    private string? _text;
    public string Text
    {
        get => _text ?? "";
        set => SetProperty(ref _text, value);
    }

    [ObservableProperty]
    private int _wpm;

    public EditTextTypeActionViewModel() { }

    public EditTextTypeActionViewModel(TextTypeAction action)
    {
        _text = action.Text;
        _wpm = action.WPM;
    }
}
