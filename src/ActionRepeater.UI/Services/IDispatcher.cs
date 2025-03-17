using System;

namespace ActionRepeater.UI.Services;

public interface IDispatcher
{
    void Enqueue(Action action);
}
