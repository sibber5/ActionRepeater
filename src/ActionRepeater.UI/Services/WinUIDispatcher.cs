using System;

namespace ActionRepeater.UI.Services;

public sealed class WinUIDispatcher : IDispatcher
{
    private readonly WindowProperties _windowProperties;

    public WinUIDispatcher(WindowProperties windowProperties)
    {
        _windowProperties = windowProperties;
    }

    public void Enqueue(Action action)
    {
        _windowProperties.DispatcherQueue!.TryEnqueue(new(action));
    }
}
