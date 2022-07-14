using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ActionRepeater.Core.Input;
using ActionRepeater.Win32;
using PathWindows;

namespace ActionRepeater.UI.Services;

public class PathWindowService
{
    public bool IsPathWindowOpen => _pathWindow is not null;

    private PathWindow? _pathWindow;
    private int _lastCursorPtsCount;
    private POINT? _lastAbsPt;

    public void OpenPathWindow()
    {
        Debug.Assert(_pathWindow is null, "Path window is not null.");

        if (ActionManager.CursorPathStart is null)
        {
            _pathWindow = new();
            _lastAbsPt = null;
        }
        else
        {
            var absCursorPts = ActionManager.GetAbsoluteCursorPath().Select(p => (System.Drawing.Point)p.MovPoint).ToArray();
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

            var cursorPath = ActionManager.CursorPath;

            _lastCursorPtsCount = cursorPath.Count;

            while (_pathWindow is not null)
            {
                await Task.Delay(40);

                if (_lastCursorPtsCount == cursorPath.Count) continue;

                if (_pathWindow is null) break;

                if (cursorPath.Count == 0)
                {
                    _lastAbsPt = ActionManager.CursorPathStart?.MovPoint;
                    _lastCursorPtsCount = cursorPath.Count;
                    _pathWindow.ClearPath();
                    continue;
                }

                _lastAbsPt ??= ActionManager.CursorPathStart!.MovPoint;

                for (int i = _lastCursorPtsCount; i < cursorPath.Count; ++i)
                {
                    var newPoint = Core.Action.MouseMovement.OffsetPointWithinScreens(_lastAbsPt.Value, cursorPath[i].MovPoint);
                    _pathWindow.AddLineToPath(_lastAbsPt.Value, newPoint);
                    _lastAbsPt = newPoint;
                }

                _lastCursorPtsCount = cursorPath.Count;
            }

            Debug.WriteLine("Update Path Window Task Finished.");
        });
    }
}
