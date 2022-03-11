using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ActionRepeater.Action;
using System.Diagnostics;

namespace ActionRepeater.Input;

internal static class Player
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

    public static void PlayActions(IReadOnlyList<InputAction> actions)
    {
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();
        Task.Run(async () =>
        {
            for (int i = 0; i < actions.Count; ++i)
            {
                if (_tokenSource.IsCancellationRequested)
                {
                    break;
                }

                if (actions[i] is WaitAction w)
                {
                    // await contiuation task to avoid TaskCancellationException
                    await Task.Delay(w.Duration, _tokenSource.Token).ContinueWith(task => { });
                    continue;
                }

                actions[i].Play();
            }
        }, _tokenSource.Token).ContinueWith(task =>
        {
            Debug.WriteLine("Finished play task.");
            App.MainWindow.DispatcherQueue.TryEnqueue(() => IsPlaying = false);

            Debug.WriteLine("Disposing token source...");
            _tokenSource!.Dispose();
            _tokenSource = null;
        });
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
