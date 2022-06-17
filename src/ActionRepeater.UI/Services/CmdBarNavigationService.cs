using System;
using ActionRepeater.UI.Views;
using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ActionRepeater.UI.Services;

public sealed class CmdBarNavigationService : INotifyPropertyChanged
{
    public enum Tag
    {
        Home,
        Options
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void RaisePropertyChanged([CallerMemberName] string propertyName = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private UserControl _currentCmdBarView = null!;
    public UserControl CurrentCmdBarView
    {
        get => _currentCmdBarView;
        private set
        {
            _currentCmdBarView = value;
            RaisePropertyChanged();
        }
    }

    private readonly Lazy<CmdBarHomeView> _homeViewLazy = new(() => new() { ViewModel = new CmdBarHomeViewModel() });

    private readonly Lazy<CmdBarOptionsView> _optionsViewLazy = new(() => new() { ViewModel = new CmdBarOptionsViewModel() });

    public void Navigate(Tag tag)
    {
        CurrentCmdBarView = tag switch
        {
            Tag.Home => _homeViewLazy.Value,
            Tag.Options => _optionsViewLazy.Value,
            _ => throw new ArgumentException("tag isn't valid.")
        };
    }

    public void Navigate(string? tag)
    {
        CurrentCmdBarView = tag switch
        {
            nameof(Tag.Home) => _homeViewLazy.Value,
            nameof(Tag.Options) => _optionsViewLazy.Value,
            _ => throw new ArgumentException("tag isn't valid.")
        };
    }
}
