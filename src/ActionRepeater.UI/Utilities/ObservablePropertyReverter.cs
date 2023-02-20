using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ActionRepeater.UI.Services;

namespace ActionRepeater.UI.Utilities;

/// <summary>
/// Reverts an observale property after a property changing event.
/// </summary>
public sealed class ObservablePropertyReverter<T>
{
    public T PreviousValue { get; set; }

    public bool IsReverting { get; private set; } = false;

    private readonly Func<T> _getProperty;
    private readonly Action<T> _setProperty;

    private readonly IDispatcher _dispatcher;

    private Action? _revertFunc;
    private Action? _revertDispatched;

    public ObservablePropertyReverter(T previousValue, Func<T> getProperty, Action<T> setProperty, IDispatcher dispatcher)
    {
        PreviousValue = previousValue;
        _getProperty = getProperty;
        _setProperty = setProperty;

        _dispatcher = dispatcher;
    }

    public void Revert()
    {
        _revertFunc ??= () =>
        {
            var comparer = EqualityComparer<T>.Default;
            while (comparer.Equals(_getProperty(), PreviousValue)) { }

            _dispatcher.Enqueue(_revertDispatched ??= () =>
            {
                IsReverting = true;
                _setProperty(PreviousValue);
                IsReverting = false;
            });
        };

        Task.Run(_revertFunc);
    }
}
