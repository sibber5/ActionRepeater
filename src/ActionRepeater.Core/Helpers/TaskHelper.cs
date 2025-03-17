using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ActionRepeater.Core.Helpers;

public static class TaskHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task Run(Action<object?> action, object? state, CancellationToken cancellationToken)
        => Task.Factory.StartNew(action, state, cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
}
