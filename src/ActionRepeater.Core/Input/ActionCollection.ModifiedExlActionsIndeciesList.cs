using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Utilities;

namespace ActionRepeater.Core.Input;

public sealed partial class ActionCollection
{
    /// <summary>Contains the indecies of the modified actions in ActionsExlKeyRepeat.</summary>
    /// <remarks>
    /// A modified action is usually a <see cref="WaitAction"/> with extended duration which would be 
    /// in place of the auto repeat actions in <see cref="ActionCollection._actions"/>.
    /// </remarks>
    private sealed class ModifiedExlActionsIndeciesList : IList<int>
    {
        private readonly List<int> _indecies;

        private readonly ObservableCollectionEx<InputAction> _actions;
        private readonly ObservableCollectionEx<InputAction> _actionsExlKeyRepeat;

        public int Count => _indecies.Count;

        public bool IsReadOnly => false;

        public int this[int index] { get => _indecies[index]; set => _indecies[index] = value; }

        public ModifiedExlActionsIndeciesList(ObservableCollectionEx<InputAction> actions, ObservableCollectionEx<InputAction> actionsExlKeyRepeat)
        {
            _indecies = new();
            _actions = actions;
            _actionsExlKeyRepeat = actionsExlKeyRepeat;
        }

        public void Add(int item) => _indecies.Add(item);

        public bool Remove(int item) => _indecies.Remove(item);

        public void Clear() => _indecies.Clear();

        public int IndexOf(int item) => _indecies.IndexOf(item);

        public void RemoveAt(int index) => _indecies.RemoveAt(index);

        public bool Contains(int item) => _indecies.Contains(item);

        public IEnumerator<int> GetEnumerator() => _indecies.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _indecies.GetEnumerator();

        /// <returns>true if the action has been modified (in <see cref="ActionsExlKeyRepeat"/>).</returns>
        /// <remarks>
        /// A modified action is usually a <see cref="WaitAction"/> with extended duration which would be 
        /// in place of the auto repeat actions in <see cref="Actions"/>.
        /// </remarks>
        public bool HasActionBeenModified(InputAction action) => IndexOfModifiedAction(action) != -1;

        /// <inheritdoc cref="HasActionBeenModified(InputAction)"/>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool HasActionBeenModified(int actionIndex) => _indecies.Contains(actionIndex);


        /// <returns>The index of <paramref name="action"/> in <see cref="ActionsExlKeyRepeat"/> <b>if</b> it is a modified action, otherwise -1.</returns>
        public int IndexOfModifiedAction(InputAction action)
        {
            foreach (int idx in CollectionsMarshal.AsSpan(_indecies))
            {
                if (_actionsExlKeyRepeat[idx] == action)
                {
                    return idx;
                }
            }

            return -1;
        }

        public (int StartIndex, int Count) GetRangeTiedToModifiedActionIdx(int modifiedActionIndex)
        {
            int startIdx = _actions.AsSpan().RefIndexOfReverse(_actionsExlKeyRepeat[modifiedActionIndex - 1]) + 1;
            if (startIdx == 0) throw new InvalidOperationException($"action {nameof(ActionCollection._actionsExlKeyRepeat)}[{modifiedActionIndex - 1}] doesnt exist in {nameof(ActionCollection._actions)}.");

            int endIdx = _actions.AsSpan().RefIndexOfReverse(_actionsExlKeyRepeat[modifiedActionIndex + 1]);
            if (endIdx == -1) throw new InvalidOperationException($"action {nameof(ActionCollection._actionsExlKeyRepeat)}[{modifiedActionIndex + 1}] doesnt exist in {nameof(ActionCollection._actions)}.");

            int count = endIdx - startIdx;

            return (startIdx, count);
        }

        public IEnumerable<InputAction> GetActionsFromRange(int startIndex, int count)
        {
            for (int i = 0; i < count; i++) yield return _actions[startIndex++];
        }

        public IEnumerable<InputAction> GetActionsFromRange((int StartIndex, int Count) rangeTuple) => GetActionsFromRange(rangeTuple.StartIndex, rangeTuple.Count);

        /// <returns>A sequence of the ranges (start indecies and count) that are tied to each modified action.</returns>
        public IEnumerable<(int StartIndex, int Count)> GetRangesTiedToModifiedActions()
        {
            foreach (int idx in _indecies) yield return GetRangeTiedToModifiedActionIdx(idx);
        }

        /// <returns>A sequence of the <see cref="InputAction"/>s that are tied to each modified action.</returns>
        public IEnumerable<IEnumerable<InputAction>> GetActionsTiedToModifiedActions()
        {
            foreach (int idx in _indecies) yield return GetActionsFromRange(GetRangeTiedToModifiedActionIdx(idx));
        }

        public bool IsActionTiedToModifiedAction(InputAction action) => GetActionsTiedToModifiedActions().Any(x => x.Contains(action, ReferenceEqualityComparer.Instance));

        public void CopyTo(int[] array, int arrayIndex) => _indecies.CopyTo(array, arrayIndex);

        void IList<int>.Insert(int index, int item) => throw new NotSupportedException();
    }
}
