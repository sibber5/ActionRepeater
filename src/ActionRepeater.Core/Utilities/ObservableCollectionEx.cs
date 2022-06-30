using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ActionRepeater.Core.Utilities;

public class ObservableCollectionEx<T> : ObservableCollection<T>
{
    protected static class EventArgsCache
    {
        public static readonly PropertyChangedEventArgs CountPropertyChanged = new("Count");
        public static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");
        public static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
    }

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
                base.OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
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

    public void AddRange(IEnumerable<T> collection, bool sendAddActionEvenIfRequiresAllocatingList = false, bool raiseEvents = true)
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

        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

        if (collection is IList<T> rangeList)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, rangeList, startIndex));
        }
        else if (sendAddActionEvenIfRequiresAllocatingList)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T>(collection), startIndex));
        }
        else
        {
            OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
        }
    }

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
        if (SuppressCollectionChanged)
        {
            _collectionChangedSuppressed = true;
            return;
        }

        base.OnCollectionChanged(e);
    }
}
