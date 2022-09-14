using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;

namespace ActionRepeater.Core.Input;

public sealed class Player
{
    /// <summary>
    /// Note: This is usually invoked from a thread pool thread.
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


    private readonly Func<Task> _playInputActionsAsync;

    private readonly Action<Task> _cleanUp;


    private readonly ActionCollection _actionCollection;

    public Player(ActionCollection actionCollection)
    {
        _actionCollection = actionCollection;

        _playInputActionsAsync = async () =>
        {
            for (int i = 0; i < _actionsToPlay!.Count; ++i)
            {
                if (_tokenSource!.IsCancellationRequested) return;

                UpdateView?.Invoke(IsAutoRepeatAction(_actionsToPlay[i]) || (i > 0 && _actionsToPlay[i] is WaitAction && IsAutoRepeatAction(_actionsToPlay[i - 1])));

                if (_actionsToPlay[i] is WaitAction w)
                {
                    try
                    {
                        await Task.Delay(w.Duration, _tokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        Debug.WriteLine($"[{nameof(Player)}] Delay task was cancelled.");
                        return;
                    }
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

        var playInputActions = repeatCount == 1 ? _playInputActionsAsync : async () =>
        {
            if (repeatCount < 0) while (true)
            {
                await _playInputActionsAsync();
            }
            else for (int i = 0; i < repeatCount; ++i)
            {
                await _playInputActionsAsync();
            }
        };

        IsPlaying = true;
        Debug.WriteLine("[Player] Started play task.");

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
        if (isPathRelative)
        {
            return Task.Run(async () =>
            {
                if (_tokenSource!.IsCancellationRequested) return;

                await Task.Delay(_actionCollection.CursorPathStart!.DelayDuration, _tokenSource!.Token).ContinueWith(task => { });
                InputSimulator.MoveMouse(_actionCollection.CursorPathStart!.Delta, false, false);

                for (int i = 0; i < path.Count; ++i)
                {
                    if (_tokenSource!.IsCancellationRequested) return;

                    await Task.Delay(path[i].DelayDuration, _tokenSource!.Token).ContinueWith(task => { });
                    InputSimulator.MoveMouse(path[i].Delta, true);
                }
            }, _tokenSource!.Token);
        }

        return Task.Run(async () =>
        {
            for (int i = 0; i < path.Count; ++i)
            {
                if (_tokenSource!.IsCancellationRequested) return;

                await Task.Delay(path[i].DelayDuration, _tokenSource!.Token).ContinueWith(task => { });
                InputSimulator.MoveMouse(path[i].Delta, false, false);
            }
        }, _tokenSource!.Token);
    }

    public void Cancel()
    {
        Debug.Assert(_tokenSource is not null, $"{nameof(_tokenSource)} is null. A play task may not have been run.");

        Debug.WriteLine($"[{nameof(Player)}] Cancelling play task...");
        _tokenSource!.Cancel();
    }

    public void RefreshIsPlaying() => IsPlayingChanged?.Invoke(null, _isPlaying);
}
