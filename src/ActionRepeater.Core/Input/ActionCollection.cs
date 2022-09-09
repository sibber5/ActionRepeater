using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Helpers;
using ActionRepeater.Core.Utilities;

namespace ActionRepeater.Core.Input;

public sealed partial class ActionCollection : ICollection<InputAction>
{
    public const string ActionTiedToModifiedActMsg = "The specified action is tied to a modified action (an action that represents multiple actions, e.g. a wait action that represents key auto repeat actions).";

    // TODO: refactor cursor path stuff to a seperate class
    public event EventHandler<MouseMovement?>? CursorPathStartChanged;

    public event NotifyCollectionChangedEventHandler? ActionsCountChanged;

    public event NotifyCollectionChangedEventHandler? ActionCollectionChanged
    {
        add => _actions.CollectionChanged += value;
        remove => _actions.CollectionChanged -= value;
    }


    private MouseMovement? _cursorPathStart;
    /// <remarks>
    /// If this is null then <see cref="CursorPath"/> is empty;
    /// and if <see cref="CursorPath"/> is not empty then this is not null.
    /// </remarks>
    public MouseMovement? CursorPathStart
    {
        get => _cursorPathStart;
        set
        {
            if (value == _cursorPathStart) return;

            _cursorPathStart = value;
            CursorPathStartChanged?.Invoke(null, value);
        }
    }
    
    public List<MouseMovement> CursorPath { get; } = new();

    public IReadOnlyList<InputAction> Actions => _actions;
    public IReadOnlyList<InputAction> ActionsExlKeyRepeat => _actionsExlKeyRepeat;

    private readonly ObservableCollectionEx<InputAction> _actions = new();
    private readonly ObservableCollectionEx<InputAction> _actionsExlKeyRepeat = new();


    private readonly ModifiedExlActionsIndeciesList _moddedExlActsIdxs;


    public ActionCollection()
    {
        _moddedExlActsIdxs = new(_actions, _actionsExlKeyRepeat);

        ActionCollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add
                || e.Action == NotifyCollectionChangedAction.Remove
                || e.Action == NotifyCollectionChangedAction.Reset)
            {
                ActionsCountChanged?.Invoke(s, e);
            }
        };

        // this *could* cause a memory leak *if* Options.Instance should live longer than this action collection instance,
        // *but* this will not happen with the current usage of these, as this is registered as a singleton.
        // if for some reason in the future that changed this class would implement IDisposable and unsubscribe from the event in Dispose.
        Options.Instance.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName!.Equals(nameof(Options.Instance.UseCursorPosOnClicks), StringComparison.Ordinal))
            {
                bool newVal = Options.Instance.UseCursorPosOnClicks;
                foreach (InputAction action in _actions)
                {
                    if (action is MouseButtonAction mbAction)
                    {
                        mbAction.UsePosition = newVal;
                    }
                }
            }
        };
    }


    public IReadOnlyList<MouseMovement> GetAbsoluteCursorPath()
    {
        if (CursorPathStart is null) return Array.Empty<MouseMovement>();

        List<MouseMovement> absPath = new();

        absPath.Add(CursorPathStart);

        for (int i = 0; i < CursorPath.Count; ++i)
        {
            MouseMovement delta = CursorPath[i];
            MouseMovement lastAbs = absPath[^1];

            Win32.POINT pt = MouseMovement.OffsetPointWithinScreens(lastAbs.MovPoint, delta.MovPoint);

            if (pt == lastAbs.MovPoint)
            {
                lastAbs.ActualDelay += delta.ActualDelay;
                continue;
            }

            absPath.Add(new MouseMovement(pt, delta.ActualDelay));
        }

        return absPath;
    }


    public void LoadActionData(ActionData data)
    {
        Clear();

        if (data.Actions is not null)
        {
            _actions.AddRange(data.Actions);
        }

        if (!data.CursorPathRelative.IsNullOrEmpty())
        {
            if (data.CursorPathStartAbs is null)
                throw new ArgumentException($"There is no start position ({nameof(data.CursorPathStartAbs)} is null), but the cursor path is not empty.", nameof(data));

            CursorPathStart = data.CursorPathStartAbs;
            CursorPath.AddRange(data.CursorPathRelative!);
        }
        else if (data.CursorPathStartAbs is not null)
        {
            throw new ArgumentException($"The cursor path is not empty, but there is a start position ({nameof(data.CursorPathStartAbs)} is not null).", nameof(data));
        }

        FillFilteredActionList();
    }

    /// <summary>Fills ActionsEclKeyRepeat from the actinos in Actions.</summary>
    public void FillFilteredActionList()
    {
        _actionsExlKeyRepeat.SuppressNotifications = true;
        _actionsExlKeyRepeat.Clear();
        _moddedExlActsIdxs.Clear();

        for (int i = 0; i < _actions.Count; ++i)
        {
            InputAction action = _actions[i];

            if (action is WaitAction waitAction)
            {
                AddOrUpdateWaitActionInExl(waitAction, _actions[i - 1]);
                continue;
            }

            if (!(action is KeyAction keyAction && keyAction.IsAutoRepeat))
            {
                _actionsExlKeyRepeat.Add(action);
            }
        }

        _actionsExlKeyRepeat.SuppressNotifications = false;
    }

    // TODO: Implement ActionCollection.Insert
    public void Add(InputAction item) => Add(item, false);

    public void Add(InputAction item, bool addAutoRepeatIfActIsKeyUp)
    {
        if (item is WaitAction waitAction)
        {
            if (_actions.Count > 0 && _actions[^1] is WaitAction lastWaitAction)
            {
                lastWaitAction.Duration += waitAction.Duration;
            }
            else
            {
                _actions.Add(item);
            }

            AddOrUpdateWaitActionInExl(waitAction, _actions[^1]);

            return;
        }

        _actions.Add(item);

        if (item is not KeyAction ka || !ka.IsAutoRepeat)
        {
            _actionsExlKeyRepeat.Add(item);
        }

        if (addAutoRepeatIfActIsKeyUp && item is KeyAction keyAction && keyAction.ActionType == KeyActionType.KeyUp)
        {
            KeyAction? lastKeyDownAct = (KeyAction?)_actions
                .LastOrDefault(x => x is KeyAction xAsKeyAct
                                    && xAsKeyAct.ActionType == KeyActionType.KeyDown
                                    && xAsKeyAct.Key == keyAction.Key
                                    && !xAsKeyAct.IsAutoRepeat);

            if (lastKeyDownAct is not null)
            {
                InsertKeyAutoRepeatActions(lastKeyDownAct, keyAction);
            }
        }
    }

    public bool Remove(InputAction item) => TryRemove(item) is null;

    /// <returns>The message if removing failed, otherwise null.</returns>
    public string? TryRemove(InputAction action)
    {
        int exlIndex = _moddedExlActsIdxs.IndexOfModifiedAction(action);
        if (exlIndex != -1)
        {
            var (startIdx, count) = _moddedExlActsIdxs.GetRangeTiedToModifiedActionIdx(exlIndex);

            // TODO: add ObservableCollectionEx.RemoveRange
            _actions.SuppressNotifications = true;
            for (int i = 0; i < count; i++)
            {
                _actions.RemoveAt(startIdx);
            }
            _actions.SuppressNotifications = false;

            _actionsExlKeyRepeat.RemoveAt(exlIndex);

            return null;
        }

        if (_moddedExlActsIdxs.IsActionTiedToModifiedAction(action)) return ActionTiedToModifiedActMsg;

        int idx = _actions.RefIndexOfReverse(action);
        int exlIdx = _actionsExlKeyRepeat.RefIndexOfReverse(action);

        if (action is KeyAction keyAct && keyAct.ActionType == KeyActionType.KeyDown)
        {
            UpdateKeyAutoRepeatActions(idx, null);
        }

        _actions.RemoveAt(idx);
        _actionsExlKeyRepeat.RemoveAt(exlIdx);

        // merge adjacent wait actions
        if (idx < _actions.Count && exlIdx < _actionsExlKeyRepeat.Count
            && _actions[idx] is WaitAction wa && _actionsExlKeyRepeat[exlIdx] is WaitAction exlWa
            && wa.Equals(exlWa))
        {
            _moddedExlActsIdxs.Remove(exlIdx);

            if (!ReferenceEquals(wa, exlWa)) _actionsExlKeyRepeat[exlIdx] = wa;

            // merge current and next wait action if possible
            if (idx + 1 < _actions.Count && exlIdx + 1 < _actionsExlKeyRepeat.Count
                && _actions[idx + 1] is WaitAction nextWa
                && _actions[idx + 1] == _actionsExlKeyRepeat[exlIdx + 1])
            {
                wa.Duration += nextWa.Duration;

                _actions.RemoveAt(idx + 1);
                _actionsExlKeyRepeat.RemoveAt(exlIdx + 1);
            }

            // merge current and previous wait action if possible
            if (idx > 0 && exlIdx > 0
                && _actions[idx - 1] is WaitAction prevWa
                && _actions[idx - 1] == _actionsExlKeyRepeat[exlIdx - 1])
            {
                prevWa.Duration += wa.Duration;

                _actions.RemoveAt(idx);
                _actionsExlKeyRepeat.RemoveAt(exlIdx);
            }
        }

        return null;
    }

    /// <param name="isFilteredList">true if the action to replace is in the filtered list (<see cref="_actionsExlKeyRepeat"/>)</param>
    /// <param name="index">The index of the action to replace in the list.</param>
    /// <returns>The message if replacing failed, otherwise null.</returns>
    public string? TryReplace(bool isFilteredList, int index, InputAction newAction)
    {
        InputAction actionToReplace;
        int replacedActionIdx;

        if (isFilteredList)
        {
            if (_moddedExlActsIdxs.HasActionBeenModified(index))
            {
                if (newAction is not WaitAction) throw new NotSupportedException("modified action is not WaitAction.");

                var (startIdx, count) = _moddedExlActsIdxs.GetRangeTiedToModifiedActionIdx(index);

                // TODO: add ObservableCollectionEx.RemoveRange
                _actions.SuppressNotifications = true;
                for (int i = 0; i < count; i++)
                {
                    _actions.RemoveAt(startIdx);
                }

                _actionsExlKeyRepeat[index] = newAction;
                _actions.Insert(startIdx, newAction);

                InsertKeyAutoRepeatActions((KeyAction)_actions[startIdx - 1], (KeyAction)_actions[startIdx + 1]);

                _actions.SuppressNotifications = false;

                return null;
            }

            actionToReplace = _actionsExlKeyRepeat[index];

            _actionsExlKeyRepeat[index] = newAction;

            replacedActionIdx = _actions.RefIndexOfReverse(actionToReplace);
            if (replacedActionIdx == -1) throw new NotImplementedException("selected item not available in Actions.");
            _actions[replacedActionIdx] = newAction;

            return null;
        }

        actionToReplace = _actions[index];

        if (_moddedExlActsIdxs.IsActionTiedToModifiedAction(actionToReplace)) return ActionTiedToModifiedActMsg;

        if (actionToReplace is KeyAction keyAct && keyAct.ActionType == KeyActionType.KeyDown)
        {
            if (newAction is KeyAction newKeyAct && newKeyAct.ActionType == KeyActionType.KeyDown)
            {
                UpdateKeyAutoRepeatActions(index, newKeyAct.Key);
            }
            else
            {
                UpdateKeyAutoRepeatActions(index, null);
            }
        }

        _actions[index] = newAction;

        replacedActionIdx = _actionsExlKeyRepeat.RefIndexOfReverse(actionToReplace);
        if (replacedActionIdx == -1) throw new NotImplementedException("selected item not available in ActionsExlKeyRepeat.");
        _actionsExlKeyRepeat[replacedActionIdx] = newAction;

        return null;
    }


    public void ClearCursorPath()
    {
        CursorPathStart = null;
        CursorPath.Clear();
    }

    public void ClearActions()
    {
        _actions.Clear();
        _actionsExlKeyRepeat.Clear();
        _moddedExlActsIdxs.Clear();
    }

    public void Clear()
    {
        ClearCursorPath();
        ClearActions();
    }


    private void AddOrUpdateWaitActionInExl(WaitAction curWaitAction, InputAction prevUnfilteredAction)
    {
        int actionsExlLastIdx = _actionsExlKeyRepeat.Count - 1;
        if (_actionsExlKeyRepeat.Count > 0 && _actionsExlKeyRepeat[actionsExlLastIdx] is WaitAction lastFilteredWaitAction)
        {
            // if there is a modified wait action for _actionsExlKeyRepeat, update it. otherwise create one.
            // (see _modifiedFilteredActionsIdxs remarks for what a modified wait action is)
            if (_moddedExlActsIdxs.HasActionBeenModified(actionsExlLastIdx))
            {
                lastFilteredWaitAction.Duration += curWaitAction.Duration;
            }
            else if (!ReferenceEquals(prevUnfilteredAction, lastFilteredWaitAction))
            {
                _actionsExlKeyRepeat[actionsExlLastIdx] = new WaitAction(lastFilteredWaitAction.Duration + curWaitAction.Duration);
                _moddedExlActsIdxs.Add(actionsExlLastIdx);
            }
        }
        else
        {
            _actionsExlKeyRepeat.Add(curWaitAction);
        }
    }

    /// <summary>
    /// Adds key auto repeat actions between the specified key down and key up actions.
    /// </summary>
    /// <remarks>
    /// Both actions passed must be in <see cref="_actions"/>.<br/>
    /// <paramref name="keyUpAction"/> must come after <paramref name="keyDownAction"/> in the collection.
    /// </remarks>
    private void InsertKeyAutoRepeatActions(KeyAction keyDownAction, KeyAction keyUpAction)
    {
        int keyDownActIdx = _actions.RefIndexOfReverse(keyDownAction);
        int keyUpActIdxCountOffset = _actions.RefIndexOfReverse(keyUpAction) - _actions.Count; // use offset from end because the count will change

        int iterationsToSkip = 0; // to skip the key repeat actions we add (because we know what they are)
        bool keyDelayAdded = false;
        int curWaitDuration = 0;
        for (int i = keyDownActIdx + 1; i < _actions.Count + keyUpActIdxCountOffset; i++)
        {
            if (iterationsToSkip > 0)
            {
                iterationsToSkip--;
                continue;
            }

            if (_actions[i] is not WaitAction waitAct) continue;

            curWaitDuration += waitAct.Duration;

            if (!keyDelayAdded)
            {
                InsertAutoRepeatActionAfterXTime(Win32.SystemInformation.KeyRepeatDelayMS);

                keyDelayAdded = true;
                continue;
            }

            InsertAutoRepeatActionAfterXTime(Win32.SystemInformation.KeyRepeatInterval);

            // Adds a key auto repeat action after the specified milliseonds.
            // e.g. if milliseconds == 500 (0.5s), and there was a wait action for 2 seconds (2000 ms)
            // it would be split into one 0.5s, and then the auto repeat actions would be added, and
            // then the rest of the wait duration of 1.5s.
            void InsertAutoRepeatActionAfterXTime(double milliseconds)
            {
                if (AreWaitDurationsNearlyEqual(milliseconds, curWaitDuration))
                {
                    // if there are more than one auto repeat actions, insert this one below them.
                    int insertIndex = i + 1;
                    for (int j = insertIndex; j < _actions.Count; j++)
                    {
                        if (!(_actions[j] is KeyAction ka && ka.IsAutoRepeat))
                        {
                            insertIndex = j;
                            break;
                        }
                    }
                    _actions.Insert(insertIndex, new KeyAction(KeyActionType.KeyDown, keyUpAction.Key, true));

                    iterationsToSkip = 1;
                    curWaitDuration = 0;
                }
                else if (curWaitDuration > milliseconds)
                {
                    int durationToKeyDelay = (int)Math.Round(milliseconds) - (curWaitDuration - waitAct.Duration);
                    int durationLeft = waitAct.Duration - durationToKeyDelay;

                    // create a modified wait action for _actionsExlKeyRepeat
                    // (see _modifiedFilteredActionsIdxs remarks for what a modified wait action is)
                    int actionsExlWaitIdx = _actionsExlKeyRepeat.RefIndexOfReverse(waitAct);
                    if (actionsExlWaitIdx != -1)
                    {
                        _actionsExlKeyRepeat[actionsExlWaitIdx] = new WaitAction(waitAct.Duration);
                        _moddedExlActsIdxs.Add(actionsExlWaitIdx);
                    }

                    waitAct.Duration = durationToKeyDelay;
                    _actions.Insert(i + 1, new KeyAction(KeyActionType.KeyDown, keyUpAction.Key, true));
                    _actions.Insert(i + 2, new WaitAction(durationLeft));

                    iterationsToSkip = 1;
                    curWaitDuration = 0;
                }

                static bool AreWaitDurationsNearlyEqual(double duration, int durationToCompare)
                {
                    int floor = (int)Math.Floor(duration / 10) * 10;
                    int ceiling = (int)Math.Ceiling(duration / 10) * 10;

                    return durationToCompare >= floor - 4 && durationToCompare <= ceiling + 4;
                }
            }
        }
    }

    /// <summary>
    /// If <paramref name="newKey"/> is null, auto repeat actions will be removed.
    /// </summary>
    /// <param name="keyDownIndex">Index (in <see cref="_actions"/>) of the key down action (not auto repeat).</param>
    public void UpdateKeyAutoRepeatActions(int keyDownIndex, Win32.Input.VirtualKey? newKey)
    {
        int startIdx = keyDownIndex + 1;

        int end = _actions.Count;
        for (int i = startIdx; i < _actions.Count; i++)
        {
            if (_actions[i] is KeyAction ka && ka.ActionType == KeyActionType.KeyUp && ka.Key == ((KeyAction)_actions[keyDownIndex]).Key)
            {
                end = i;
                break;
            }
        }

        if (newKey != null)
        {
            for (int i = startIdx; i < end; i++)
            {
                if (_actions[i] is not KeyAction keyAct || !keyAct.IsAutoRepeat) continue;

                keyAct.Key = newKey.Value;
            }

            return;
        }

        _actions.SuppressNotifications = true;

        // remove auto repeat actions
        int curIndex = startIdx;
        for (int i = startIdx; i < end; i++)
        {
            bool removed = false;

            if (_actions[curIndex] is KeyAction keyAct && keyAct.IsAutoRepeat)
            {
                _actions.RemoveAt(curIndex);

                if (curIndex >= _actions.Count) break;

                removed = true;
            }

            // merge adjacent wait actions
            if (_actions[curIndex] is WaitAction curWaitAct && _actions[curIndex - 1] is WaitAction prevWaitAct)
            {
                prevWaitAct.Duration += curWaitAct.Duration;
                _actions.RemoveAt(curIndex);

                removed = true;
            }

            if (!removed) curIndex++;

            if (curIndex >= _actions.Count) break;
        }

        _actions.SuppressNotifications = false;
    }


    public bool HasActionBeenModified(InputAction action) => _moddedExlActsIdxs.HasActionBeenModified(action);

    public bool IsActionTiedToModifiedAction(InputAction action) => _moddedExlActsIdxs.IsActionTiedToModifiedAction(action);

    bool ICollection<InputAction>.IsReadOnly => false;

    int ICollection<InputAction>.Count => _actions.Count;

    bool ICollection<InputAction>.Contains(InputAction item) => _actions.Contains(item);

    void ICollection<InputAction>.CopyTo(InputAction[] array, int arrayIndex) => _actions.CopyTo(array, arrayIndex);

    IEnumerator IEnumerable.GetEnumerator() => _actions.GetEnumerator();

    IEnumerator<InputAction> IEnumerable<InputAction>.GetEnumerator() => _actions.GetEnumerator();
}
