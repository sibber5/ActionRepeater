using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ActionRepeater.Action;
using Debug = System.Diagnostics.Debug;

namespace ActionRepeater.Input;

internal static class Player
{
    private static CancellationTokenSource _tokenSource = null;

    public static bool IsPlaying { get; private set; } = false;
    public static event EventHandler<bool> IsPlayingChanged;

    public static void PlayActions(IEnumerable<InputAction> actions)
    {
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            if (_tokenSource.IsCancellationRequested)
            {
                return;
            }

            foreach (var action in actions)
            {
                if (action is WaitAction a)
                {
                    await Task.Delay(a.Duration, _tokenSource.Token).ContinueWith(_ => { });
                }
                else
                {
                    action.Play();
                }

                if (_tokenSource.IsCancellationRequested)
                {
                    return;
                }
            }
        }, _tokenSource.Token).ContinueWith(_ =>
        {
            Debug.WriteLine("Finished play task.");
            IsPlaying = false;
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                IsPlayingChanged?.Invoke(null, false);
                CleanUp();
            });
        });
        IsPlaying = true;
        IsPlayingChanged?.Invoke(null, true);
        Debug.WriteLine("Started Playing");
    }

    public static void Cancel()
    {
        Debug.WriteLine("Cancelling play task...");
        _tokenSource.Cancel();
    }

    private static void CleanUp()
    {
        _tokenSource.Dispose();
        _tokenSource = null;
    }
}
