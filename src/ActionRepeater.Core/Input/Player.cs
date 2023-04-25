using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Helpers;
using ActionRepeater.Core.Utilities;
using ActionRepeater.Win32.Synch.Utilities;

namespace ActionRepeater.Core.Input;

public sealed class Player
{
    /// <summary>
    /// <code>
    /// OnActionPlayed(InputAction? currentAction, int index)
    /// </code>
    /// </summary>
    public event Action<InputAction?, int>? ActionPlayed;

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

    private CancellationTokenSource? _tokenSource;


    private IReadOnlyList<InputAction>? _actionsToPlay;
    private IReadOnlyList<MouseMovement>? _cursorPath;

    private Func<Player, Task>? _runPlayActions;

    private readonly Action<object?> _playInputActions;
    private Action<object?>? _playCursorMovement;

    private readonly Action<Task> _cleanUp;

    private readonly CoreOptions _options;

    private readonly ActionCollection _actionCollection;

    private readonly HighResolutionWaiter _actionsWaiter;
    private readonly HighResolutionWaiter _cursorMovementWaiter;

    public Player(CoreOptions options, ActionCollection actionCollection, HighResolutionWaiter actionsWaiter, HighResolutionWaiter cursorMovementWaiter)
    {
        if (ReferenceEquals(actionsWaiter, cursorMovementWaiter)) throw new ArgumentException($"{nameof(actionsWaiter)} must be a different instance than {nameof(cursorMovementWaiter)}");

        _options = options;
        _actionCollection = actionCollection;
        _actionsWaiter = actionsWaiter;
        _cursorMovementWaiter = cursorMovementWaiter;

        _playInputActions = static (state) =>
        {
            var p = (Player)state!;

            ReadOnlySpan<InputAction> actionsSpan = ((ObservableCollectionEx<InputAction>)p._actionsToPlay!).AsSpan();

            for (int i = 0; i < actionsSpan.Length; ++i)
            {
                if (p._tokenSource!.IsCancellationRequested) return;

                InputAction action = actionsSpan[i];

                p.ActionPlayed?.Invoke(action, i);

                if (action is WaitableInputAction waitableAction)
                {
                    waitableAction.PlayWait(p._actionsWaiter, p._tokenSource.Token);
                    continue;
                }

                action.Play();
            }
        };

        _cleanUp = task =>
        {
            Debug.WriteLine($"[{nameof(Player)}] Finished play task.");

            ActionPlayed?.Invoke(null, default);
            IsPlaying = false;

            _tokenSource!.Dispose();
            _tokenSource = null;
            Debug.WriteLine($"[{nameof(Player)}] Disposed token source.");
        };
    }

    /// <returns>
    /// false if there are no actions to play or its already playing, otherwise true.
    /// </returns>
    public bool PlayActions()
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

        var actions = _options.SendKeyAutoRepeat ? _actionCollection.Actions : _actionCollection.FilteredActions;
        IReadOnlyList<MouseMovement>? cursorPath = _options.CursorMovementMode switch
        {
            CursorMovementMode.Absolute => _actionCollection.GetAbsoluteCursorPath().ToList(),
            CursorMovementMode.Relative => _actionCollection.CursorPath,
            _ => null
        };

        PlayActionsCore(actions, cursorPath, _options.CursorMovementMode == CursorMovementMode.Relative, _options.PlayRepeatCount);

        return true;
    }

    private void PlayActionsCore(IReadOnlyList<InputAction> actions, IReadOnlyList<MouseMovement>? cursorPath, bool isPathRelative, int repeatCount = 1)
    {
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        _actionsToPlay = actions;
        _cursorPath = cursorPath;

        _playCursorMovement = GetCursorMovementFunction(isPathRelative);

        Func<Player, Task> runInputActions = static (p) => TaskHelper.Run(p._playInputActions, p, p._tokenSource!.Token);
        Func<Player, Task> runAllActions = static (p) => Task.WhenAll(
            TaskHelper.Run(p._playInputActions, p, p._tokenSource!.Token),
            TaskHelper.Run(p._playCursorMovement!, p, p._tokenSource!.Token)
        );

        _runPlayActions = _actionCollection.CursorPathStart is not null && _cursorPath?.Count > 0
            ? runAllActions
            : runInputActions;

        IsPlaying = true;
        Debug.WriteLine($"[{nameof(Player)}] Started play task.");

        if (repeatCount == 1)
        {
            _runPlayActions(this).ContinueWith(_cleanUp);
        }
        else
        {
            Task.Run(async () =>
            {
                if (repeatCount < 0) while (!_tokenSource.IsCancellationRequested)
                {
                    await _runPlayActions(this);
                }
                else for (int i = 0; i < repeatCount && !_tokenSource.IsCancellationRequested; ++i)
                {
                    await _runPlayActions(this);
                }
            }, _tokenSource.Token).ContinueWith(_cleanUp);
        }
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

    private static Action<object?> GetCursorMovementFunction(bool isPathRelative) => isPathRelative
    ? static (state) =>
    {
        var p = (Player)state!;

        if (p._tokenSource!.IsCancellationRequested) return;

        ReadOnlySpan<MouseMovement> cursorPathSpan = CollectionsMarshal.AsSpan((List<MouseMovement>)p._cursorPath!);

        InputSimulator.MoveMouse(p._actionCollection.CursorPathStart!.Value.Delta, relativePos: false);

        foreach (MouseMovement mouseMovement in cursorPathSpan)
        {
            if (p._tokenSource!.IsCancellationRequested) return;

            p._cursorMovementWaiter.WaitNS(mouseMovement.DelayDurationNS);
            InputSimulator.MoveMouse(mouseMovement.Delta, relativePos: true);
        }
    }
    : static (state) =>
    {
        var p = (Player)state!;

        ReadOnlySpan<MouseMovement> cursorPathSpan = CollectionsMarshal.AsSpan((List<MouseMovement>)p._cursorPath!);

        foreach (MouseMovement mouseMovement in cursorPathSpan)
        {
            if (p._tokenSource!.IsCancellationRequested) return;

            p._cursorMovementWaiter.WaitNS(mouseMovement.DelayDurationNS);
            InputSimulator.MoveMouse(mouseMovement.Delta, relativePos: false);
        }
    };

}
