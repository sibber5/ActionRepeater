using System.ComponentModel;
using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views;

public sealed partial class EditActionView : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private EditActionViewModel? _viewModel;
    public EditActionViewModel ViewModel
    {
        get => _viewModel!;
        set
        {
            if (_viewModel is not null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            value.PropertyChanged += ViewModel_PropertyChanged;
            _viewModel = value;
        }
    }

    public UserControl? CurrentEditActionView => ViewModel.CurrentEditActionViewModel switch
    {
        EditKeyActionViewModel vm => new EditKeyActionView() { ViewModel = vm },
        EditMouseButtonActionViewModel vm => new EditMouseButtonActionView() { ViewModel = vm },
        EditMouseWheelActionViewModel vm => new EditMouseWheelActionView() { ViewModel = vm },
        EditWaitActionViewModel vm => new EditWaitActionView() { ViewModel = vm },
        _ => null
    };

    private PropertyChangedEventArgs? _currentViewChanged;

    public EditActionView()
    {
        InitializeComponent();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.CurrentEditActionViewModel))
        {
            _currentViewChanged ??= new(nameof(CurrentEditActionView));
            PropertyChanged?.Invoke(this, _currentViewChanged);
        }
    }
}
