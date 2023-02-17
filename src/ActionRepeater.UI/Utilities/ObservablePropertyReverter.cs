using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace ActionRepeater.UI.Utilities;

public sealed class ObservablePropertyReverter<T>
{
    public T PreviousValue { get; set; }

    public bool IsReverting { get; private set; } = false;

    private readonly Func<T> _getProperty;
    private readonly Action<T> _setProperty;

    private Func<Task>? _revertFunc;
    private DispatcherQueueHandler? _revertDispatched;

    public ObservablePropertyReverter(T previousValue, Func<T> getProperty, Action<T> setProperty)
    {
        PreviousValue = previousValue;
        _getProperty = getProperty;
        _setProperty = setProperty;
    }

    public void Revert()
    {
        _revertFunc ??= async () =>
        {
            var comparer = EqualityComparer<T>.Default;
            while (comparer.Equals(_getProperty(), PreviousValue)) await Task.Delay(5);
            App.Current.MainWindow.DispatcherQueue.TryEnqueue(_revertDispatched ??= () =>
            {
                IsReverting = true;
                _setProperty(PreviousValue);
                IsReverting = false;
            });
        };

        Task.Run(_revertFunc);
    }
}
