using System;
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
public class SyncedObservableCollection<T, TSource> : ObservableCollectionEx<T>
{
    private readonly ObservableCollection<TSource?> _sourceCol;
    private readonly Func<TSource?, T> _createT;

    /// <summary>
    /// Initializes a new instance of the class that is synchronized with <paramref name="sourceCol"/>.
    /// </summary>
    /// <param name="sourceCol">The collection that this class would be synchronized with.</param>
    /// <param name="createT">A function that creates a <typeparamref name="T"/> instance, optionally using a <typeparamref name="TSource"/> instance.</param>
    public SyncedObservableCollection(ObservableCollection<TSource?> sourceCol, Func<TSource?, T> createT)
    {
        _sourceCol = sourceCol;
        _createT = createT;

        _sourceCol.CollectionChanged += SourceCollectionChanged;
    }

    private void SourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
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
                if (_sourceCol.Count == 0)
                {
                    base.ClearItems();
                    break;
                }

                CheckReentrancy();

                Items.Clear();
                for (int i = 0; i < _sourceCol.Count; i++)
                {
                    Items.Add(_createT(_sourceCol[i]));
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
        throw new NotSupportedException();
    }

    protected override void ClearItems()
    {
        _sourceCol.Clear();
        base.ClearItems();
    }

    protected override void RemoveItem(int index)
    {
        _sourceCol.RemoveAt(index);
        base.RemoveItem(index);
    }

    protected override void SetItem(int index, T item)
    {
        throw new NotSupportedException();
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        _sourceCol.Move(oldIndex, newIndex);
        base.MoveItem(oldIndex, newIndex);
    }
}
