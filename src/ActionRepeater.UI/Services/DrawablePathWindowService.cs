using System;
using System.Diagnostics;
using System.Linq;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.Services.Interop;
using ActionRepeater.Win32;
using ActionRepeater.Win32.Utilities;

namespace ActionRepeater.UI.Services;

public sealed class DrawablePathWindowService : IDisposable
{
    public event Action? WindowOpened;
    public event Action? WindowClosed;

    public bool IsWindowOpen => _windowHost.IsWindowOpen;

    private ActionCollection _actionCollection;
    private WindowHostWrapper _windowHost;

    private int _cursorSpeedFactor;

    private bool _disposed;

    public DrawablePathWindowService(ActionCollection actionCollection)
    {
        _actionCollection = actionCollection;
    }

    public unsafe void OpenWindow(int cursorSpeedFactor)
    {
        _cursorSpeedFactor = cursorSpeedFactor;
        _windowHost.OpenDrawablePathWindow(
            _actionCollection.GetAbsoluteCursorPath().Select(x => new MouseMovement(ScreenCoordsConverter.GetVirtScreenPosFromPosRelToPrimary(x.Delta), x.DelayDurationNS)).ToArray(),
            OnWindowClosing
        );

        WindowOpened?.Invoke();
    }

    private unsafe void OnWindowClosing(MouseMovement* points, int length)
    {
        if (length == 0)
        {
            WindowClosed?.Invoke();
            return;
        }

        Span<MouseMovement> cursorPath = new(points, length);

        for (int i = 0; i < cursorPath.Length; i++)
        {
            var mov = cursorPath[i];

            MouseMovement newMov = new(
                ScreenCoordsConverter.GetPosRelToPrimaryFromVirtScreenPoint(mov.Delta),
                mov.DelayDurationNS
            );

            cursorPath[i] = newMov;
        }

        Debug.Assert(cursorPath[0] == _actionCollection.CursorPathStart || _actionCollection.CursorPathStart is null);

        ToRelativePath(cursorPath);

        int lastPointIndex;

        if (_actionCollection.CursorPathStart is null)
        {
            lastPointIndex = 0;
            _actionCollection.CursorPathStart = cursorPath[0];
        }
        else
        {
            lastPointIndex = cursorPath.LastIndexOf(_actionCollection.CursorPath[^1]);
        }

        var newPoints = cursorPath[(lastPointIndex + 1)..];

        foreach (var mov in newPoints) _actionCollection.CursorPath.Add(new(mov.Delta, mov.DelayDurationNS * _cursorSpeedFactor));

        WindowClosed?.Invoke();

        static void ToRelativePath(Span<MouseMovement> absolutePath)
        {
            for (int i = absolutePath.Length - 1; i >= 1; i--)
            {
                var cur = absolutePath[i].Delta;
                var prev = absolutePath[i - 1].Delta;
                POINT rel = new(cur.x - prev.x, cur.y - prev.y);

                absolutePath[i] = new(rel, absolutePath[i].DelayDurationNS);
            }
        }
    }

    private void DisposeCore()
    {
        if (!_disposed)
        {
            _windowHost.Dispose();
            _actionCollection = null!;

            _disposed = true;
        }
    }

    ~DrawablePathWindowService()
    {
        DisposeCore();
    }

    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }
}
