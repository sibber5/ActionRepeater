using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Utilities;
using ActionRepeater.Win32.Synch.Utilities;

namespace ActionRepeater.Core.Input;

public sealed class Player
{
    /// <summary>
    /// Note: This is usually invoked from a different thread than the main thread.
    /// </summary>
    public event EventHandler<bool>? IsPlayingChanged;

    private bool _isPlaying;
    public bool IsPlaying
    {
        get => _isPlaying;
        private set
        {
            _isPlaying = value;
            IsPlayingChanged?.Invoke(this, value);
        }
    }

    public Action<InputAction?, int>? OnActionPlayed;

    private CancellationTokenSource? _tokenSource;


    private IReadOnlyList<InputAction>? _actionsToPlay;

    private IReadOnlyList<MouseMovement>? _cursorPath;

    private readonly Func<Task> _runAllActions;
    private readonly Func<Task> _runInputActions;
    private Func<Task>? _taskToRun;

    private System.Action? _playCursorMovementRelative;
    private System.Action? _playCursorMovementAbsolute;
    private System.Action? _playCursorMovement;

    private readonly System.Action _playInputActions;

    private readonly Action<Task> _cleanUp;


    private readonly ActionCollection _actionCollection;

    private readonly HighResolutionWaiter _actionsWaiter;
    private readonly HighResolutionWaiter _cursorMovementWaiter;

    public Player(ActionCollection actionCollection, HighResolutionWaiter actionsWaiter, HighResolutionWaiter cursorMovementWaiter)
    {
        if (ReferenceEquals(actionsWaiter, cursorMovementWaiter)) throw new ArgumentException($"{nameof(actionsWaiter)} must be a different instance than {nameof(cursorMovementWaiter)}");

        _actionCollection = actionCollection;
        _actionsWaiter = actionsWaiter;
        _cursorMovementWaiter = cursorMovementWaiter;

        _playInputActions = () =>
        {
            ReadOnlySpan<InputAction> actionsSpan = ((ObservableCollectionEx<InputAction>)_actionsToPlay!).AsSpan();

            for (int i = 0; i < actionsSpan.Length; ++i)
            {
                if (_tokenSource!.IsCancellationRequested) return;

                InputAction action = actionsSpan[i];

                OnActionPlayed?.Invoke(action, i);

                if (action is WaitableInputAction w)
                {
                    w.PlayWait(_actionsWaiter);
                    continue;
                }

                action.Play();
            }

            OnActionPlayed?.Invoke(null, default);
        };

        _cleanUp = task =>
        {
            Debug.WriteLine($"[{nameof(Player)}] Finished play task.");

            IsPlaying = false;

            _tokenSource!.Dispose();
            _tokenSource = null;
            Debug.WriteLine($"[{nameof(Player)}] Disposed token source.");
        };

        _runAllActions = () => Task.WhenAll(Task.Run(_playInputActions, _tokenSource!.Token), Task.Run(_playCursorMovement!, _tokenSource!.Token));
        _runInputActions = () => Task.Run(_playInputActions, _tokenSource!.Token);
    }

    /// <returns>
    /// false if there are no actions to play or its already playing, otherwise true.
    /// </returns>
    public bool TryPlayActions()
    {
        if (_actionCollection.Actions.Count == 0 && _actionCollection.CursorPathStart is null)
        {
            Debug.Assert(_actionCollection.CursorPath.Count == 0, $"{nameof(_actionCollection.CursorPath)} is not empty.");
            return false;
        }

        if (IsPlaying)
        {
            Cancel();
            return false;
        }

        var actions = CoreOptions.Instance.SendKeyAutoRepeat ? _actionCollection.Actions : _actionCollection.ActionsExlKeyRepeat;
        IReadOnlyList<MouseMovement>? cursorPath = CoreOptions.Instance.CursorMovementMode switch
        {
            CursorMovementMode.Absolute => _actionCollection.GetAbsoluteCursorPath().ToList(),
            CursorMovementMode.Relative => _actionCollection.CursorPath,
            _ => null
        };

        PlayActions(actions, cursorPath, CoreOptions.Instance.CursorMovementMode == CursorMovementMode.Relative, CoreOptions.Instance.PlayRepeatCount);

        return true;
    }

    private void PlayActions(IReadOnlyList<InputAction> actions, IReadOnlyList<MouseMovement>? cursorPath, bool isPathRelative, int repeatCount = 1)
    {
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        _actionsToPlay = actions;
        _cursorPath = cursorPath;

        SetCursorMovementFunction(isPathRelative);

        _taskToRun = _actionCollection.CursorPathStart is not null && cursorPath?.Count > 0
            ? _runAllActions
            : _runInputActions;

        IsPlaying = true;
        Debug.WriteLine($"[{nameof(Player)}] Started play task.");

        if (repeatCount == 1)
        {
            _taskToRun().ContinueWith(_cleanUp);
        }
        else
        {
            Task.Run(async () =>
            {
                if (repeatCount < 0) while (!_tokenSource.IsCancellationRequested)
                    {
                        await _taskToRun();
                    }
                else for (int i = 0; i < repeatCount && !_tokenSource.IsCancellationRequested; ++i)
                    {
                        await _taskToRun();
                    }
            }).ContinueWith(_cleanUp);
        }
    }

    private void SetCursorMovementFunction(bool isPathRelative)
    {
        _playCursorMovement = isPathRelative
            ? _playCursorMovementRelative ??= () =>
            {
                if (_tokenSource!.IsCancellationRequested) return;

                ReadOnlySpan<MouseMovement> cursorPathSpan = CollectionsMarshal.AsSpan((List<MouseMovement>)_cursorPath!);

                InputSimulator.MoveMouse(_actionCollection.CursorPathStart!.Delta, relativePos: false);

                foreach (MouseMovement mouseMovement in cursorPathSpan)
                {
                    if (_tokenSource!.IsCancellationRequested) return;

                    _cursorMovementWaiter.WaitNS(mouseMovement.DelayDurationNS);
                    InputSimulator.MoveMouse(mouseMovement.Delta, relativePos: true);
                }
            }
        : _playCursorMovementAbsolute ??= () =>
        {
            ReadOnlySpan<MouseMovement> cursorPathSpan = CollectionsMarshal.AsSpan((List<MouseMovement>)_cursorPath!);

            foreach (MouseMovement mouseMovement in cursorPathSpan)
            {
                if (_tokenSource!.IsCancellationRequested) return;

                _cursorMovementWaiter.WaitNS(mouseMovement.DelayDurationNS);
                InputSimulator.MoveMouse(mouseMovement.Delta, relativePos: false);
            }
        };
    }

    public void Cancel()
    {
        Debug.Assert(_tokenSource is not null, $"{nameof(_tokenSource)} is null. A play task may not have been run.");

        Debug.WriteLine($"[{nameof(Player)}] Cancelling play task...");
        _tokenSource!.Cancel();
        _actionsWaiter.Cancel();
        _cursorMovementWaiter.Cancel();
    }

    public void RefreshIsPlaying() => IsPlayingChanged?.Invoke(this, _isPlaying);
}
