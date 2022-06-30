using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ActionRepeater.Core.Utilities;

namespace ActionRepeater.UI.Utilities;

public class ViewModelCollection<TViewModel, TModel> : ObservableCollectionEx<TViewModel>
{
    private readonly ObservableCollection<TModel?> _modelCol;
    private readonly Func<TModel?, TViewModel> _createVM;

    public ViewModelCollection(ObservableCollection<TModel?> modelCol, Func<TModel?, TViewModel> createVM)
    {
        _modelCol = modelCol;
        _createVM = createVM;

        _modelCol.CollectionChanged += ModelCollectionChanged;
    }

    private void ModelCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                CheckReentrancy();

                int startIndex = Count;
                TViewModel?[] newVMs = new TViewModel?[e.NewItems!.Count];

                for (int i = 0; i < e.NewItems!.Count; i++)
                {
                    var vm = _createVM((TModel?)e.NewItems![i]);
                    newVMs[i] = vm;
                    Items.Add(vm);
                }

                OnPropertyChanged(EventArgsCache.CountPropertyChanged);
                OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newVMs, startIndex));
                break;

            case NotifyCollectionChangedAction.Remove:
                base.RemoveItem(e.OldStartingIndex);
                break;

            case NotifyCollectionChangedAction.Replace:
                base.SetItem(e.OldStartingIndex, _createVM((TModel?)e.NewItems![0]));
                break;

            case NotifyCollectionChangedAction.Move:
                base.MoveItem(e.OldStartingIndex, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Reset:
                if (_modelCol.Count == 0)
                {
                    base.ClearItems();
                    break;
                }

                CheckReentrancy();

                Items.Clear();
                for (int i = 0; i < _modelCol.Count; i++)
                {
                    Items.Add(_createVM(_modelCol[i]));
                }

                OnPropertyChanged(EventArgsCache.CountPropertyChanged);
                OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
                OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
                break;

            default:
                throw new NotImplementedException();
        }
    }

    protected override void InsertItem(int index, TViewModel item)
    {
        throw new NotSupportedException();
    }

    protected override void ClearItems()
    {
        _modelCol.Clear();
        base.ClearItems();
    }

    protected override void RemoveItem(int index)
    {
        _modelCol.RemoveAt(index);
        base.RemoveItem(index);
    }

    protected override void SetItem(int index, TViewModel item)
    {
        throw new NotSupportedException();
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        _modelCol.Move(oldIndex, newIndex);
        base.MoveItem(oldIndex, newIndex);
    }
}
