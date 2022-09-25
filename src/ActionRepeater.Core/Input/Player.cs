using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;
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
            IsPlayingChanged?.Invoke(null, value);
        }
    }

    /// <summary>
    /// void UpdateView(bool isAutoRepeat)
    /// </summary>
    public Action<bool>? UpdateView { get; set; }


    private CancellationTokenSource? _tokenSource;


    private IReadOnlyList<InputAction>? _actionsToPlay;


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
            for (int i = 0; i < _actionsToPlay!.Count; ++i)
            {
                if (_tokenSource!.IsCancellationRequested) return;

                UpdateView?.Invoke(IsAutoRepeatAction(_actionsToPlay[i]) || (i > 0 && _actionsToPlay[i] is WaitAction && IsAutoRepeatAction(_actionsToPlay[i - 1])));

                if (_actionsToPlay[i] is WaitAction w)
                {
                    _actionsWaiter.Wait((uint)w.Duration);
                    continue;
                }

                _actionsToPlay[i].Play();
            }

            // this will deselect the last action item in the list view
            UpdateView?.Invoke(false);

            static bool IsAutoRepeatAction(InputAction a) => a is KeyAction keyAction && keyAction.IsAutoRepeat;
        };

        _cleanUp = task =>
        {
            Debug.WriteLine($"[{nameof(Player)}] Finished play task.");

            IsPlaying = false;

            _tokenSource!.Dispose();
            _tokenSource = null;
            Debug.WriteLine($"[{nameof(Player)}] Disposed token source.");
        };
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

        var actions = Options.Instance.SendKeyAutoRepeat ? _actionCollection.Actions : _actionCollection.ActionsExlKeyRepeat;
        var cursorPath = Options.Instance.CursorMovementMode switch
        {
            CursorMovementMode.Absolute => _actionCollection.GetAbsoluteCursorPath(),
            CursorMovementMode.Relative => _actionCollection.CursorPath,
            _ => null
        };

        PlayActions(actions, cursorPath, Options.Instance.CursorMovementMode == CursorMovementMode.Relative, Options.Instance.PlayRepeatCount);

        return true;
    }

    public void PlayActions(IReadOnlyList<InputAction> actions, IReadOnlyList<MouseMovement>? path, bool isPathRelative, int repeatCount = 1)
    {
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        _actionsToPlay = actions;

        var playInputActions = repeatCount == 1 ? _playInputActions : () =>
        {
            if (repeatCount < 0) while (!_tokenSource.IsCancellationRequested)
            {
                _playInputActions();
            }
            else for (int i = 0; i < repeatCount && !_tokenSource.IsCancellationRequested; ++i)
            {
                _playInputActions();
            }
        };

        IsPlaying = true;
        Debug.WriteLine($"[{nameof(Player)}] Started play task.");

        if (_actionCollection.CursorPathStart is not null && path?.Count > 0)
        {
            Task.WhenAll(Task.Run(playInputActions, _tokenSource.Token), PlayCursorMovement(path, isPathRelative))
                .ContinueWith(_cleanUp);
        }
        else
        {
            Task.Run(playInputActions, _tokenSource.Token)
                .ContinueWith(_cleanUp);
        }
    }

    // TODO: repeat based on repeatCount; maybe cache the tasks; use a high resolution waitable timer instead of Task.Delay.
    private Task PlayCursorMovement(IReadOnlyList<MouseMovement> path, bool isPathRelative)
    {
        return Task.Run(isPathRelative
            ? () =>
            {
                if (_tokenSource!.IsCancellationRequested) return;

                InputSimulator.MoveMouse(_actionCollection.CursorPathStart!.Delta, false, false);

                for (int i = 0; i < path.Count; ++i)
                {
                    if (_tokenSource!.IsCancellationRequested) return;

                    _cursorMovementWaiter.Wait((uint)path[i].DelayDuration);
                    InputSimulator.MoveMouse(path[i].Delta, true);
                }
            }
            : () =>
            {
                for (int i = 0; i < path.Count; ++i)
                {
                    if (_tokenSource!.IsCancellationRequested) return;

                    _cursorMovementWaiter.Wait((uint)path[i].DelayDuration);
                    InputSimulator.MoveMouse(path[i].Delta, false, false);
                }
            }, _tokenSource!.Token);
    }

    public void Cancel()
    {
        Debug.Assert(_tokenSource is not null, $"{nameof(_tokenSource)} is null. A play task may not have been run.");

        Debug.WriteLine($"[{nameof(Player)}] Cancelling play task...");
        _tokenSource!.Cancel();
        _actionsWaiter.Cancel();
        _cursorMovementWaiter.Cancel();
    }

    public void RefreshIsPlaying() => IsPlayingChanged?.Invoke(null, _isPlaying);
}
