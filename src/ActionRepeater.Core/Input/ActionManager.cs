using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Helpers;
using ActionRepeater.Core.Utilities;

namespace ActionRepeater.Core.Input;

public static class ActionManager
{
    static ActionManager()
    {
        Recorder.ReplaceLastAction = (act) =>
        {
            // the caller of this func always checks if the action list is not empty
            if (_actions[^1] == _actionsExlKeyRepeat[^1])
            {
                _actionsExlKeyRepeat[^1] = act;
            }
            _actions[^1] = act;
        };

        ActionCollectionChanged += static (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add
                || e.Action == NotifyCollectionChangedAction.Remove
                || e.Action == NotifyCollectionChangedAction.Reset)
            {
                ActionsCountChanged?.Invoke(s, e);
            }
        };
    }

    public static event EventHandler<MouseMovement?>? CursorPathStartChanged;

    private static MouseMovement? _cursorPathStart;
    /// <remarks>
    /// If this is null then <see cref="CursorPath"/> is empty;
    /// and if <see cref="CursorPath"/> is not empty then this is not null.
    /// </remarks>
    public static MouseMovement? CursorPathStart
    {
        get => _cursorPathStart;
        set
        {
            if (value == _cursorPathStart) return;

            _cursorPathStart = value;
            CursorPathStartChanged?.Invoke(null, value);
        }
    }
    public static List<MouseMovement> CursorPath { get; } = new();

    private static readonly ObservableCollectionEx<InputAction> _actions = new();
    public static IReadOnlyList<InputAction> Actions => _actions;

    private static readonly ObservableCollectionEx<InputAction> _actionsExlKeyRepeat = new();
    public static IReadOnlyList<InputAction> ActionsExlKeyRepeat => _actionsExlKeyRepeat;

    public static event NotifyCollectionChangedEventHandler? ActionCollectionChanged
    {
        add => _actions.CollectionChanged += value;
        remove => _actions.CollectionChanged -= value;
    }

    public static event NotifyCollectionChangedEventHandler? ActionsCountChanged;

    /// <summary>Contains the indecies of the modified actions in ActionsExlKeyRepeat.</summary>
    /// <remarks>
    /// A modified action is usually a <see cref="WaitAction"/> with extended duration which would be 
    /// in place of the auto repeat actions in <see cref="_actions"/>.
    /// </remarks>
    private static readonly List<int> _modifiedFilteredActionsIdxs = new();

    public static IReadOnlyList<MouseMovement> GetAbsoluteCursorPath()
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

    public static void LoadActionData(ActionData data)
    {
        ClearAll();

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

    public static void AddAction(InputAction action, bool addAutoRepeatIfActIsKeyUp = false)
    {
        if (action is WaitAction waitAction)
        {
            if (_actions.Count > 0 && _actions[^1] is WaitAction lastWaitAction)
            {
                lastWaitAction.Duration += waitAction.Duration;
            }
            else
            {
                _actions.Add(action);
            }

            AddOrUpdateWaitActionInExl(waitAction, _actions[^1]);

            return;
        }

        if (action is not KeyAction ka || !ka.IsAutoRepeat)
        {
            _actionsExlKeyRepeat.Add(action);
        }

        if (addAutoRepeatIfActIsKeyUp && action is KeyAction keyAction && keyAction.ActionType == KeyActionType.KeyUp)
        {
            // TODO: fix bug with action collections and views

            KeyAction? lastKeyDownAct = (KeyAction?)_actions
                .LastOrDefault(x => x is KeyAction xAsKeyAct
                                    && xAsKeyAct.ActionType == KeyActionType.KeyDown
                                    && xAsKeyAct.Key == keyAction.Key
                                    && !xAsKeyAct.IsAutoRepeat);

            if (lastKeyDownAct is not null)
            {
                AddKeyAutoRepeatActions(lastKeyDownAct, keyAction);
            }
        }

        _actions.Add(action);
    }

    /// <summary>
    /// Adds key auto repeat actions between the specified key down and key up actions.
    /// </summary>
    /// <remarks>
    /// Both actions passed must be in <see cref="_actions"/>.<br/>
    /// <paramref name="keyUpAction"/> must come after <paramref name="keyDownAction"/> in the collection.
    /// </remarks>
    private static void AddKeyAutoRepeatActions(KeyAction keyDownAction, KeyAction keyUpAction)
    {
        int lastKeyDownActIdx = _actions.RefIndexOfReverse(keyDownAction);

        int iterationsToSkip = 0; // to skip the key repeat actions we add (because we know what they are)
        bool keyDelayAdded = false;
        int curWaitDuration = 0;
        for (int i = lastKeyDownActIdx + 1; i < _actions.Count; i++)
        {
            if (iterationsToSkip > 0)
            {
                iterationsToSkip--;
                continue;
            }

            InputAction act = _actions[i];

            if (act is not WaitAction waitAct) continue;

            curWaitDuration += waitAct.Duration;

            if (!keyDelayAdded)
            {
                AddAutoRepeatActionAfterXTime(Win32.SystemInformation.KeyRepeatDelayMS);

                keyDelayAdded = true;
                continue;
            }

            AddAutoRepeatActionAfterXTime(Win32.SystemInformation.KeyRepeatInterval);

            // Adds a key auto repeat action after the specified milliseonds.
            // e.g. if milliseconds == 500 (0.5s), and there was a wait action for 2 seconds (2000 ms)
            // it would be split into one 0.5s, and then the auto repeat actions would be added, and
            // then the rest of the wait duration of 1.5s.
            void AddAutoRepeatActionAfterXTime(double milliseconds)
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
                        _modifiedFilteredActionsIdxs.Add(actionsExlWaitIdx);
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

    /// <summary>Fills ActionsEclKeyRepeat from the actinos in Actions.</summary>
    public static void FillFilteredActionList()
    {
        _actionsExlKeyRepeat.SuppressNotifications = true;
        _actionsExlKeyRepeat.Clear();
        _modifiedFilteredActionsIdxs.Clear();

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

    private static void AddOrUpdateWaitActionInExl(WaitAction curWaitAction, InputAction prevUnfilteredAction)
    {
        int actionsExlLastIdx = _actionsExlKeyRepeat.Count - 1;
        if (_actionsExlKeyRepeat.Count > 0 && _actionsExlKeyRepeat[actionsExlLastIdx] is WaitAction lastFilteredWaitAction)
        {
            // if there is a modified wait action for _actionsExlKeyRepeat, update it. otherwise create one.
            // (see _modifiedFilteredActionsIdxs remarks for what a modified wait action is)
            if (_modifiedFilteredActionsIdxs.Contains(actionsExlLastIdx))
            {
                lastFilteredWaitAction.Duration += curWaitAction.Duration;
            }
            else if (!ReferenceEquals(prevUnfilteredAction, lastFilteredWaitAction))
            {
                _actionsExlKeyRepeat[actionsExlLastIdx] = new WaitAction(lastFilteredWaitAction.Duration + curWaitAction.Duration);
                _modifiedFilteredActionsIdxs.Add(actionsExlLastIdx);
            }
        }
        else
        {
            _actionsExlKeyRepeat.Add(curWaitAction);
        }
    }

    /// <returns>true if the action has been modified (in <see cref="_actionsExlKeyRepeat"/>).</returns>
    /// <remarks>
    /// A modified action is usually a <see cref="WaitAction"/> with extended duration which would be 
    /// in place of the auto repeat actions in <see cref="_actions"/>.
    /// </remarks>
    public static bool HasActionBeenModified(InputAction action)
    {
        foreach (int idx in _modifiedFilteredActionsIdxs)
        {
            if (_actionsExlKeyRepeat[idx] == action)
            {
                return true;
            }
        }

        return false;
    }

    /// <returns>false if the action is a modified action (see remarks), otherwise true (indicating the action was removed).</returns>
    /// <remarks>
    /// A modified action is usually a <see cref="KeyAction"/> with extended duration which would be 
    /// in place of the auto repeat actions in <see cref="_actions"/>.
    /// </remarks>
    public static bool TryRemoveAction(InputAction action)
    {
        if (HasActionBeenModified(action)) return false;

        _actions.Remove(action);
        _actionsExlKeyRepeat.Remove(action);
        return true;
    }

    /// <param name="isFilteredList">true if the action to replace is in the filtered list (<see cref="_actionsExlKeyRepeat"/>)</param>
    /// <param name="index">The index of the action to replace in the list.</param>
    public static void ReplaceAction(bool isFilteredList, int index, InputAction newAction)
    {
        InputAction actionToReplace;
        int replacedActionIdx;

        if (isFilteredList)
        {
            actionToReplace = _actionsExlKeyRepeat[index];

            _actionsExlKeyRepeat[index] = newAction;

            replacedActionIdx = _actions.IndexOf(actionToReplace);
            if (replacedActionIdx == -1) throw new NotImplementedException("selected item not available in Actions.");
            _actions[replacedActionIdx] = newAction;

            return;
        }

        actionToReplace = _actions[index];

        _actions[index] = newAction;

        replacedActionIdx = _actionsExlKeyRepeat.IndexOf(actionToReplace);
        if (replacedActionIdx == -1) throw new NotImplementedException("selected item not available in ActionsExlKeyRepeat.");
        _actionsExlKeyRepeat[replacedActionIdx] = newAction;
    }

    public static void ClearCursorPath()
    {
        CursorPathStart = null;
        CursorPath.Clear();
    }

    public static void ClearActions()
    {
        _actions.Clear();
        _actionsExlKeyRepeat.Clear();
        _modifiedFilteredActionsIdxs.Clear();

        Recorder.Reset();
    }

    public static void ClearAll()
    {
        ClearCursorPath();
        ClearActions();
    }

    /// <returns>
    /// false if there are no actions to play or its already playing, otherwise true.
    /// </returns>
    public static bool TryPlayActions()
    {
        if (_actions.Count == 0 && CursorPathStart is null)
        {
            return false;
        }

        if (Player.IsPlaying)
        {
            Player.Cancel();
            return false;
        }

        var actions = Options.Instance.SendKeyAutoRepeat ? _actions : _actionsExlKeyRepeat;
        var cursorPath = Options.Instance.CursorMovementMode switch
        {
            CursorMovementMode.Absolute => GetAbsoluteCursorPath(),
            CursorMovementMode.Relative => CursorPath,
            _ => null
        };

        Player.PlayActions(actions, cursorPath, Options.Instance.CursorMovementMode == CursorMovementMode.Relative, Options.Instance.PlayRepeatCount);

        return true;
    }
}
