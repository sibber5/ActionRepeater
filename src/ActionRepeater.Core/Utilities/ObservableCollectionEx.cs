using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace ActionRepeater.Core.Utilities;

public class ObservableCollectionEx<T> : ObservableCollection<T>
{
    protected static class EventArgsCache
    {
        public static readonly PropertyChangedEventArgs CountPropertyChanged = new("Count");
        public static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");
        public static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
    }

    /// <summary>Used to run tasks that modify the collection after the CollectionChanged event.</summary>
    internal readonly ManualResetEventSlim _collectionChangedEvent = new(true);

    private bool _propertyChangedSuppressed;

    private bool _suppressPropertyChanged;
    public bool SuppressPropertyChanged
    {
        get => _suppressPropertyChanged;
        set
        {
            _suppressPropertyChanged = value;
            if (!value && _propertyChangedSuppressed)
            {
                base.OnPropertyChanged(EventArgsCache.CountPropertyChanged);
                base.OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
                _propertyChangedSuppressed = false;
            }
        }
    }

    private bool _collectionChangedSuppressed;

    private bool _suppressCollectionChanged;
    public bool SuppressCollectionChanged
    {
        get => _suppressCollectionChanged;
        set
        {
            _suppressCollectionChanged = value;
            if (!value && _collectionChangedSuppressed)
            {
                _collectionChangedEvent.Reset();
                base.OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
                _collectionChangedEvent.Set();

                _collectionChangedSuppressed = false;
            }
        }
    }

    public bool SuppressNotifications
    {
        get => SuppressPropertyChanged && SuppressCollectionChanged;
        set
        {
            SuppressPropertyChanged = value;
            SuppressCollectionChanged = value;
        }
    }

    public ObservableCollectionEx() : base() { }
    public ObservableCollectionEx(IEnumerable<T> collection) : base(collection) { }
    public ObservableCollectionEx(List<T> list) : base(list) { }

    /// <param name="raiseAddIfRequiresAllocation">
    /// <para>
    /// true if <see cref="NotifyCollectionChangedAction.Add"/> should be raised even
    /// if allocatin a list is required.
    /// </para>
    /// <para>default is false.</para>
    /// </param>
    /// <param name="raiseEvents">
    /// <para>Indicates wheather to raise colelction changed event(s).</para>
    /// <para>default is true.</para>
    /// </param>
    public void AddRange(IEnumerable<T> collection, bool raiseAddIfRequiresAllocation = false, bool raiseEvents = true)
    {
        System.Diagnostics.Debug.Assert(collection is not null, $"{nameof(collection)} is null.");

        CheckReentrancy();

        int startIndex = Count;

        int index = Count;
        foreach (T item in collection)
        {
            Items.Insert(index++, item);
        }

        if (index == startIndex) return;

        if (!raiseEvents) return;

        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);

        if (collection is System.Collections.IList rangeList)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, rangeList, startIndex));
        }
        else if (raiseAddIfRequiresAllocation)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T>(collection), startIndex));
        }
        else
        {
            OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
        }
    }

    /// <summary>
    /// Gets a <see cref="Span{T}"/> view over the data in the collection. Items should not be added or removed from the <see cref="ObservableCollectionEx{T}"/> while the <see cref="Span{T}"/> is in use.
    /// </summary>
    /// <returns>
    /// A <see cref="Span{T}"/> instance over the <see cref="ObservableCollectionEx{T}"/>.
    /// </returns>
    public Span<T> AsSpan() => CollectionsMarshal.AsSpan((List<T>)Items);

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (SuppressPropertyChanged)
        {
            _propertyChangedSuppressed = true;
            return;
        }

        base.OnPropertyChanged(e);
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        _collectionChangedEvent.Reset();

        if (SuppressCollectionChanged)
        {
            _collectionChangedSuppressed = true;
            return;
        }

        base.OnCollectionChanged(e);

        _collectionChangedEvent.Set();
    }
}
