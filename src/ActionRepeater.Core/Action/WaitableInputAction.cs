using System;
using System.Threading;
using ActionRepeater.Win32.Synch.Utilities;

namespace ActionRepeater.Core.Action;

public abstract class WaitableInputAction : InputAction
{
    public override void Play() => throw new NotSupportedException($"Use {nameof(PlayWait)}");

    public abstract void PlayWait(HighResolutionWaiter waiter, CancellationToken cancellationToken);
}
