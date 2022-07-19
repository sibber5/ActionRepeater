using System;
using System.Collections.Generic;
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

    public static event System.Collections.Specialized.NotifyCollectionChangedEventHandler? ActionCollectionChanged
    {
        add => _actions.CollectionChanged += value;
        remove => _actions.CollectionChanged -= value;
    }

    /// <summary>Contains the indecies of the modified actions in ActionsExlKeyRepeat.</summary>
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

        if (!data.CursorPathRel.IsNullOrEmpty())
        {
            if (data.CursorPathStartAbs is null)
                throw new ArgumentException($"There is no start position ({nameof(data.CursorPathStartAbs)} is null), but the cursor path is not empty.", nameof(data));

            CursorPathStart = data.CursorPathStartAbs;
            CursorPath.AddRange(data.CursorPathRel!);
        }
        else if (data.CursorPathStartAbs is not null)
        {
            throw new ArgumentException($"The cursor path is not empty, but there is a start position ({nameof(data.CursorPathStartAbs)} is not null).", nameof(data));
        }

        FillFilteredActionList();
    }

    public static void AddAction(InputAction action)
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

            int lastFilteredActionIdx = _actionsExlKeyRepeat.Count - 1;
            if (_actionsExlKeyRepeat.Count > 0 && _actionsExlKeyRepeat[lastFilteredActionIdx] is WaitAction lastFilteredWaitAction)
            {
                if (_modifiedFilteredActionsIdxs.Contains(lastFilteredActionIdx))
                {
                    lastFilteredWaitAction.Duration += waitAction.Duration;
                }
                else if (!ReferenceEquals(_actions[^1], lastFilteredWaitAction))
                {
                    _actionsExlKeyRepeat[lastFilteredActionIdx] = new WaitAction(lastFilteredWaitAction.Duration + waitAction.Duration);
                    _modifiedFilteredActionsIdxs.Add(lastFilteredActionIdx);
                }
            }
            else
            {
                _actionsExlKeyRepeat.Add(action);
            }

            return;
        }

        _actions.Add(action);

        if (!(action is KeyAction keyAction && keyAction.IsAutoRepeat))
        {
            _actionsExlKeyRepeat.Add(action);
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
                int lastFilteredActionIdx = _actionsExlKeyRepeat.Count - 1;
                if (_actionsExlKeyRepeat.Count > 0 && _actionsExlKeyRepeat[lastFilteredActionIdx] is WaitAction lastFilteredWaitAction)
                {
                    if (_modifiedFilteredActionsIdxs.Contains(lastFilteredActionIdx))
                    {
                        lastFilteredWaitAction.Duration += waitAction.Duration;
                    }
                    else if (!ReferenceEquals(_actions[i - 1], lastFilteredWaitAction))
                    {
                        _actionsExlKeyRepeat[lastFilteredActionIdx] = new WaitAction(lastFilteredWaitAction.Duration + waitAction.Duration);
                        _modifiedFilteredActionsIdxs.Add(lastFilteredActionIdx);
                    }
                }
                else
                {
                    _actionsExlKeyRepeat.Add(action);
                }

                continue;
            }

            if (!(action is KeyAction keyAction && keyAction.IsAutoRepeat))
            {
                _actionsExlKeyRepeat.Add(action);
            }
        }

        _actionsExlKeyRepeat.SuppressNotifications = false;
    }

    /// <returns>true if the action has been modified (in <see cref="_actionsExlKeyRepeat"/>).</returns>
    /// <remarks>
    /// A modified action is usually a <see cref="KeyAction"/> with extended duration which would be 
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
