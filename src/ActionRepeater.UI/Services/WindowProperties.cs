using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace ActionRepeater.UI.Services;

/// <summary>
/// Used to avoid circular dependencies. e.g. MainWindow -> MainViewModel -> FilePicker -> MainWindow
/// </summary>
public sealed class WindowProperties
{
    public nint Handle { get; set; }
    
    public DispatcherQueue? DispatcherQueue { get; set; }

    public XamlRoot? XamlRoot { get; set; }
}
