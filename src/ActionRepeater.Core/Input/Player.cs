using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using ActionRepeater.Core.Action;

namespace ActionRepeater.Core.Input;

public static class Player
{
    public static Action<System.Action>? ExecuteOnUIThread { get; set; }

    public static Action<bool>? UpdateView { get; set; }

    private static CancellationTokenSource? _tokenSource;

    private static bool _isPlaying;
    public static bool IsPlaying
    {
        get => _isPlaying;
        private set
        {
            _isPlaying = value;
            IsPlayingChanged?.Invoke(null, value);
        }
    }

    public static event EventHandler<bool>? IsPlayingChanged;

    private static IReadOnlyList<InputAction>? _actionsToPlay;

    private static Func<Task>? _playInputActionsAsync;

    private static Action<Task>? _cleanUp;

    public static void PlayActions(IReadOnlyList<InputAction> actions, IReadOnlyList<MouseMovement>? path, bool isPathRelative)
    {
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        _actionsToPlay = actions;

        _playInputActionsAsync ??= static async () =>
        {
            for (int i = 0; i < _actionsToPlay.Count; ++i)
            {
                if (_tokenSource.IsCancellationRequested) return;

                UpdateView?.Invoke(IsAutoRepeatAction(_actionsToPlay[i]) || (i > 0 && _actionsToPlay[i] is WaitAction && IsAutoRepeatAction(_actionsToPlay[i - 1])));

                if (_actionsToPlay[i] is WaitAction w)
                {
                    try
                    {
                        await Task.Delay(w.Duration, _tokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        Debug.WriteLine("[Player] Delay task was cancelled.");
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

        _cleanUp ??= static task =>
        {
            Debug.WriteLine("[Player] Finished play task.");

            if (ExecuteOnUIThread is null)
            {
                IsPlaying = false;
            }
            else
            {
                ExecuteOnUIThread(static () => IsPlaying = false);
            }

            _tokenSource!.Dispose();
            _tokenSource = null;
            Debug.WriteLine("[Player] Disposed token source.");
        };

        IsPlaying = true;
        Debug.WriteLine("[Player] Started play task.");

        if (ActionManager.CursorPathStart is not null && path?.Count > 0)
        {
            Task.WhenAll(Task.Run(_playInputActionsAsync, _tokenSource.Token), PlayCursorMovement(path, isPathRelative))
                .ContinueWith(_cleanUp);
        }
        else
        {
            Task.Run(_playInputActionsAsync, _tokenSource.Token)
                .ContinueWith(_cleanUp);
        }
    }

    private static Task PlayCursorMovement(IReadOnlyList<MouseMovement> path, bool isPathRelative)
    {
        if (isPathRelative)
        {
            return Task.Run(async () =>
            {
                if (_tokenSource!.IsCancellationRequested) return;

                await Task.Delay(ActionManager.CursorPathStart!.DelayDuration, _tokenSource!.Token).ContinueWith(task => { });
                InputSimulator.MoveMouse(ActionManager.CursorPathStart!.MovPoint, false, false);

                for (int i = 0; i < path.Count; ++i)
                {
                    if (_tokenSource!.IsCancellationRequested) return;

                    await Task.Delay(path[i].DelayDuration, _tokenSource!.Token).ContinueWith(task => { });
                    InputSimulator.MoveMouse(path[i].MovPoint, true);
                }
            }, _tokenSource!.Token);
        }

        return Task.Run(async () =>
        {
            for (int i = 0; i < path.Count; ++i)
            {
                if (_tokenSource!.IsCancellationRequested) return;

                await Task.Delay(path[i].DelayDuration, _tokenSource!.Token).ContinueWith(task => { });
                InputSimulator.MoveMouse(path[i].MovPoint, false, false);
            }
        }, _tokenSource!.Token);
    }

    public static void Cancel()
    {
        Debug.Assert(_tokenSource is not null, $"{nameof(_tokenSource)} is null. A play task may not have been run.");

        Debug.WriteLine("[Player] Cancelling play task...");
        _tokenSource!.Cancel();
    }

    public static void RefreshIsPlaying() => IsPlayingChanged?.Invoke(null, _isPlaying);
}
