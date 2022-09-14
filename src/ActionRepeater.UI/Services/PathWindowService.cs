using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ActionRepeater.Core.Input;
using ActionRepeater.Win32;
using PathWindows;

namespace ActionRepeater.UI.Services;

public sealed class PathWindowService
{
    public bool IsPathWindowOpen => _pathWindow is not null;

    private readonly ActionCollection _actionCollection;

    private PathWindow? _pathWindow;
    private int _lastCursorPtsCount;
    private POINT? _lastAbsPt;

    public PathWindowService(ActionCollection actionCollection)
    {
        _actionCollection = actionCollection;
    }

    public void OpenPathWindow()
    {
        Debug.Assert(_pathWindow is null, "Path window is not null.");

        if (_actionCollection.CursorPathStart is null)
        {
            Debug.Assert(_actionCollection.CursorPath.Count == 0, $"{nameof(_actionCollection.CursorPath)} is not empty.");

            _pathWindow = new();
            _lastAbsPt = null;
        }
        else
        {
            var absCursorPts = _actionCollection.GetAbsoluteCursorPath().Select(p => (System.Drawing.Point)p.Delta).ToArray();
            _lastAbsPt = absCursorPts[^1];

            _pathWindow = new(absCursorPts);
        }

        RunUpdatePathWindowTask();
    }

    public void ClosePathWindow()
    {
        Debug.Assert(_pathWindow is not null, "Path window is null.");

        _pathWindow.Dispose();
        _pathWindow = null;
    }

    private void RunUpdatePathWindowTask()
    {
        Task.Run(async () =>
        {
            Debug.WriteLine("Update Window Task Started.");

            var cursorPath = _actionCollection.CursorPath;

            _lastCursorPtsCount = cursorPath.Count;

            while (_pathWindow is not null)
            {
                await Task.Delay(40);

                if (_lastCursorPtsCount == cursorPath.Count) continue;

                if (_pathWindow is null) break;

                if (cursorPath.Count == 0)
                {
                    _lastAbsPt = _actionCollection.CursorPathStart?.Delta;
                    _lastCursorPtsCount = cursorPath.Count;
                    _pathWindow.ClearPath();
                    continue;
                }

                _lastAbsPt ??= _actionCollection.CursorPathStart!.Delta;

                for (int i = _lastCursorPtsCount; i < cursorPath.Count; ++i)
                {
                    var newPoint = Core.Action.MouseMovement.OffsetPointWithinScreens(_lastAbsPt.Value, cursorPath[i].Delta);
                    _pathWindow.AddLineToPath(_lastAbsPt.Value, newPoint);
                    _lastAbsPt = newPoint;
                }

                _lastCursorPtsCount = cursorPath.Count;
            }

            Debug.WriteLine("Update Path Window Task Finished.");
        });
    }
}
