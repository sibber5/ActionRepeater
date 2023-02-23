using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ActionRepeater.Core.Utilities;

namespace ActionRepeater.UI.Utilities;

/// <summary>
/// An <see cref="ObservableCollection{T}"/> that is synchronized with another <see cref="ObservableCollection{TSource}"/>,
/// e.g. when an item is added/removed/replaced/etc. to the source collection, it gets added/removed/replaced/etc. in this one.
/// </summary>
/// <typeparam name="T">The type of elements in this collection.</typeparam>
/// <typeparam name="TSource">The type of elements in the source collection.</typeparam>
public sealed class SyncedObservableCollection<T, TSource> : ObservableCollectionEx<T>
{
    private readonly ObservableCollection<TSource?> _sourceCollection;
    private readonly Func<TSource?, T> _createT;

    private bool _sync = true;

    private T? _lastRemovedItem;
    private TSource? _lastRemovedSourceItem;

    /// <summary>
    /// Initializes a new instance of the class that is synchronized with <paramref name="sourceCollection"/>.
    /// </summary>
    /// <param name="sourceCollection">The collection that this class would be synchronized with.</param>
    /// <param name="createT">A function that creates a <typeparamref name="T"/> instance, optionally using a <typeparamref name="TSource"/> instance.</param>
    public SyncedObservableCollection(ObservableCollection<TSource?> sourceCollection, Func<TSource?, T> createT)
    {
        _sourceCollection = sourceCollection;
        _createT = createT;

        _sourceCollection.CollectionChanged += OnSourceCollectionChanged;
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_sync) return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                CheckReentrancy();

                int startIndex = e.NewStartingIndex;
                var newItems = new T?[e.NewItems!.Count];

                int index = startIndex;
                for (int i = 0; i < e.NewItems!.Count; i++)
                {
                    var item = _createT((TSource?)e.NewItems![i]);
                    newItems[i] = item;
                    Items.Insert(index++, item);
                }

                OnPropertyChanged(EventArgsCache.CountPropertyChanged);
                OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, startIndex));
                break;

            case NotifyCollectionChangedAction.Remove:
                base.RemoveItem(e.OldStartingIndex);
                break;

            case NotifyCollectionChangedAction.Replace:
                base.SetItem(e.OldStartingIndex, _createT((TSource?)e.NewItems![0]));
                break;

            case NotifyCollectionChangedAction.Move:
                base.MoveItem(e.OldStartingIndex, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Reset:
                if (_sourceCollection.Count == 0)
                {
                    base.ClearItems();
                    break;
                }

                CheckReentrancy();

                Items.Clear();
                for (int i = 0; i < _sourceCollection.Count; i++)
                {
                    Items.Add(_createT(_sourceCollection[i]));
                }

                OnPropertyChanged(EventArgsCache.CountPropertyChanged);
                OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
                OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
                break;

            default:
                throw new NotImplementedException();
        }
    }

    protected override void InsertItem(int index, T item)
    {
        if (EqualityComparer<T>.Default.Equals(item, _lastRemovedItem))
        {
            _sync = false;
            _sourceCollection.Insert(index, _lastRemovedSourceItem);
            _sync = true;

            base.InsertItem(index, item);

            // In case the source collection was changed while _sync was false
            // (e.g. after inserting to it in an ObservableCollectionEx.AfterCollectionChanged event handler)
            if (_sourceCollection.Count != Count) OnSourceCollectionChanged(_sourceCollection, EventArgsCache.ResetCollectionChanged);
            return;
        }

        throw new NotSupportedException("Cannot create source item.");
    }

    protected override void ClearItems()
    {
        _sync = false;
        _sourceCollection.Clear();
        _sync = true;

        base.ClearItems();

        // In case the source collection was changed while _sync was false
        if (_sourceCollection.Count != Count) OnSourceCollectionChanged(_sourceCollection, EventArgsCache.ResetCollectionChanged);
    }

    protected override void RemoveItem(int index)
    {
        _lastRemovedItem = this[index];
        _lastRemovedSourceItem = _sourceCollection[index];

        _sync = false;
        _sourceCollection.RemoveAt(index);
        _sync = true;

        base.RemoveItem(index);

        // In case the source collection was changed while _sync was false
        if (_sourceCollection.Count != Count) OnSourceCollectionChanged(_sourceCollection, EventArgsCache.ResetCollectionChanged);
    }

    protected override void SetItem(int index, T item)
    {
        throw new NotSupportedException("Cannot create source item.");
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        _sync = false;
        _sourceCollection.Move(oldIndex, newIndex);
        _sync = true;

        base.MoveItem(oldIndex, newIndex);

        // In case the source collection was changed while _sync was false
        if (_sourceCollection.Count != Count) OnSourceCollectionChanged(_sourceCollection, EventArgsCache.ResetCollectionChanged);
    }
}
