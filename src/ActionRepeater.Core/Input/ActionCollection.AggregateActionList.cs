using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Utilities;

namespace ActionRepeater.Core.Input;

public sealed partial class ActionCollection
{
    /// <summary>Contains the aggregate actions in <see cref="FilteredActions"/>.</summary>
    /// <remarks>
    /// An aggregate action is usually a <see cref="WaitAction"/> with extended duration which would be 
    /// in place of the auto repeat actions in <see cref="ActionCollection._actions"/>.
    /// </remarks>
    private sealed class AggregateActionList : IList<InputAction>
    {
        private readonly List<InputAction> _aggregateActions = new();

        private readonly ObservableCollectionEx<InputAction> _actions;
        private readonly ObservableCollectionEx<InputAction> _filteredActions;

        public AggregateActionList(ObservableCollectionEx<InputAction> actions, ObservableCollectionEx<InputAction> filteredActions)
        {
            _actions = actions;
            _filteredActions = filteredActions;
        }

        public InputAction this[int index] { get => _aggregateActions[index]; set => _aggregateActions[index] = value; }

        public int Count => _aggregateActions.Count;

        public bool IsReadOnly => false;

        public void Add(InputAction item) => _aggregateActions.Add(item);

        public void Clear() => _aggregateActions.Clear();

        public bool Contains(InputAction item) => _aggregateActions.Contains(item);

        public void CopyTo(InputAction[] array, int arrayIndex) => _aggregateActions.CopyTo(array, arrayIndex);

        public IEnumerator<InputAction> GetEnumerator() => _aggregateActions.GetEnumerator();

        public int IndexOf(InputAction item) => _aggregateActions.IndexOf(item);

        public void Insert(int index, InputAction item) => _aggregateActions.Insert(index, item);

        public bool Remove(InputAction item) => _aggregateActions.Remove(item);

        public void RemoveAt(int index) => _aggregateActions.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => _aggregateActions.GetEnumerator();

        /// <summary>
        /// WARNING: Items should not be added or removed from the action list (<c>_actions</c>) while the range is in use.
        /// </summary>
        public (int StartIndex, int Length) GetRangeTiedToAggregateAction(InputAction aggregateAction)
            => GetRangeTiedToAggregateAction(_filteredActions.AsSpan().RefIndexOfReverse(aggregateAction));

        /// <inheritdoc cref="GetRangeTiedToAggregateAction(InputAction)"/>
        public (int StartIndex, int Length) GetRangeTiedToAggregateAction(int filteredActionIndex)
        {
            Debug.Assert(_aggregateActions.Contains(_filteredActions[filteredActionIndex]));

            int startIdx = _actions.AsSpan().RefIndexOfReverse(_filteredActions[filteredActionIndex - 1]) + 1;
            if (startIdx == 0) throw new InvalidOperationException($"action {nameof(ActionCollection._filteredActions)}[{filteredActionIndex - 1}] doesnt exist in {nameof(ActionCollection._actions)}.");

            int endIdx = _actions.AsSpan().RefIndexOfReverse(_filteredActions[filteredActionIndex + 1]);
            if (endIdx == -1) throw new InvalidOperationException($"action {nameof(ActionCollection._filteredActions)}[{filteredActionIndex + 1}] doesnt exist in {nameof(ActionCollection._actions)}.");

            int length = endIdx - startIdx;

            return (startIdx, length);
        }

        /// <summary>
        /// WARNING: Items should not be added or removed from the action list (<c>_actions</c>) while the span is in use.
        /// </summary>
        public Span<InputAction> GetActionsTiedToAggregateAction(InputAction aggregateAction)
        {
            var (startIndex, length) = GetRangeTiedToAggregateAction(aggregateAction);
            return _actions.AsSpan().Slice(startIndex, length);
        }

        public bool IsActionTiedToAggregateAction(InputAction action)
        {
            foreach (var aggregateAction in _aggregateActions)
            {
                if (GetActionsTiedToAggregateAction(aggregateAction).Contains(aggregateAction)) return true;
            }

            return false;
        }
    }
}
