using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Input;
using ActionRepeater.Core.Utilities;
using ActionRepeater.Win32;
using static ActionRepeater.Win32.Utilities.ScreenCoordsConverter;

namespace ActionRepeater.UI.Services;

public sealed partial class PathWindowService : IDisposable
{
    public bool IsPathWindowOpen => _pathWindowWrapper.IsWindowOpen;

    private ActionCollection _actionCollection;
    private PathWindowWrapper _pathWindowWrapper;

    private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(20));
    private int _lastCount;
    private POINT? _lastAbsPoint;
    private Func<ValueTask>? _updatePathWindowTask;

    private bool _disposed;

    public PathWindowService(ActionCollection actionCollection)
    {
        _actionCollection = actionCollection;
    }

    public void OpenPathWindow()
    {
        Debug.Assert(!_pathWindowWrapper.IsWindowOpen);

        SystemInformation.RefreshMonitorSettings();

        if (_actionCollection.CursorPathStart is null)
        {
            Debug.Assert(_actionCollection.CursorPath.Count == 0, $"{nameof(_actionCollection.CursorPath)} is not empty.");

            _pathWindowWrapper.OpenWindow();

            _lastAbsPoint = null;
        }
        else
        {
            var absCursorPts = _actionCollection.GetAbsoluteCursorPath().Select(p => GetVirtScreenPosFromPosRelToPrimary(p.Delta)).ToArray();
            _lastAbsPoint = absCursorPts[^1];

            _pathWindowWrapper.OpenWindow(absCursorPts);
        }

        RunUpdatePathWindowTask();
    }

    public void ClosePathWindow()
    {
        Debug.Assert(_pathWindowWrapper.IsWindowOpen);

        _pathWindowWrapper.CloseWindow();
    }

    // TODO: Fix cursor path being "delayed" while path window is open
    private void RunUpdatePathWindowTask()
    {
        _updatePathWindowTask ??= async ValueTask () =>
        {
            Debug.WriteLine("Update Window Task Started.");

            var cursorPath = _actionCollection.CursorPath;

            StopwatchSlim sw = new();

            _lastCount = cursorPath.Count;
            while (_pathWindowWrapper.IsWindowOpen)
            {
                await _timer.WaitForNextTickAsync();

                if (_lastCount == cursorPath.Count) continue;

                if (!_pathWindowWrapper.IsWindowOpen) break;

                if (cursorPath.Count == 0)
                {
                    _lastCount = 0;
                    _pathWindowWrapper.ClearPath();
                    continue;
                }

                if (_lastAbsPoint is null)
                {
                    _lastAbsPoint = _actionCollection.CursorPathStart!.Delta;
                    _pathWindowWrapper.AddPoint(GetVirtScreenPosFromPosRelToPrimary(_lastAbsPoint.Value), render: true);
                    continue;
                }

                int count = cursorPath.Count;

                for (int i = _lastCount; i < count; i++)
                {
                    POINT newPoint = MouseMovement.OffsetPointWithinScreens(_lastAbsPoint.Value, cursorPath[i].Delta);
                    if (_lastAbsPoint == newPoint) continue;

                    _pathWindowWrapper.AddPoint(GetVirtScreenPosFromPosRelToPrimary(newPoint), render: false);

                    _lastAbsPoint = newPoint;
                }

                //_pathWindowWrapper.AddPoint(GetVirtScreenPosFromPosRelToPrimary(Win32.PInvoke.Helpers.GetCursorPos()), render: false);

                //sw.Restart();
                _pathWindowWrapper.RenderPath();
                //sw.Stop();
                //Debug.WriteLine($"render time: {sw.ElapsedMilliseconds}ms");

                _lastCount = count;
            }

            Debug.WriteLine("Update Path Window Task Finished.");
        };

        Task.Run(_updatePathWindowTask);
    }

    private void DisposeCore()
    {
        if (!_disposed)
        {
            _pathWindowWrapper.Dispose();
            _actionCollection = null!;

            _disposed = true;
        }
    }

    ~PathWindowService()
    {
        DisposeCore();
    }

    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }
}
