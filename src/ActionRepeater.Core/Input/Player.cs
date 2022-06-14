using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using ActionRepeater.Core.Action;

namespace ActionRepeater.Core.Input;

public static class Player
{
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

    public static void PlayActions(IReadOnlyList<InputAction> actions, IReadOnlyList<MouseMovement>? path, bool isPathRelative)
    {
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        var playInputActionsAsync = async () =>
        {
            for (int i = 0; i < actions.Count; ++i)
            {
                if (_tokenSource.IsCancellationRequested) return;

                if (actions[i] is WaitAction w)
                {
                    // await contiuation task to avoid TaskCancellationException
                    await Task.Delay(w.Duration, _tokenSource.Token).ContinueWith(task => { });
                    continue;
                }

                actions[i].Play();
            }
        };

        Action<Task> cleanUp = task =>
        {
            Debug.WriteLine("Finished play task.");

            IsPlaying = false;
            //App.MainWindow.DispatcherQueue.TryEnqueue(() => IsPlaying = false);

            Debug.WriteLine("Disposing token source...");
            _tokenSource!.Dispose();
            _tokenSource = null;
        };

        if (ActionManager.CursorPathStart is not null && path?.Count > 0)
        {
            Task.WhenAll(Task.Run(playInputActionsAsync, _tokenSource.Token), PlayCursorMovement(path, isPathRelative))
                .ContinueWith(cleanUp);
        }
        else
        {
            Task.Run(playInputActionsAsync, _tokenSource.Token).ContinueWith(cleanUp);
        }

        IsPlaying = true;
        Debug.WriteLine("Started play task.");
    }

    public static void Cancel()
    {
        Debug.Assert(_tokenSource is not null, $"{nameof(_tokenSource)} is null. A play task may not have been run.");

        Debug.WriteLine("Cancelling play task...");
        _tokenSource!.Cancel();
    }
}
