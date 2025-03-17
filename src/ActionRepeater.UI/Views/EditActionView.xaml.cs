using System;
using System.ComponentModel;
using ActionRepeater.UI.Helpers;
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

    public UserControl CurrentEditActionView => ActionMappingHelper.EditViewModelToEditView(ViewModel.CurrentEditActionViewModel);

    private readonly string _titleText;

    private PropertyChangedEventArgs? _currentViewChanged;

    /// <param name="isAddView">If true, the title of the dialog will start with "Add:" instead of "Edit:".</param>
    public EditActionView(bool isAddView = false)
    {
        _titleText = isAddView ? "Add:" : "Edit:";

        InitializeComponent();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (nameof(ViewModel.CurrentEditActionViewModel).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            _currentViewChanged ??= new(nameof(CurrentEditActionView));
            PropertyChanged?.Invoke(this, _currentViewChanged);
        }
    }
}
