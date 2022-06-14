using System;

namespace ActionRepeater.Core.Utilities;

public class TimeConsistencyChecker
{
    private const int MaxTimeDelta = 1000;
    private const int ConsistencyMargin = 150;

    private readonly bool _useLastTimeAsBaseline;

    private int? _lastTickCount;
    private int? _baseline;

    private int _tickDeltasCount;
    private int _tickDeltasTotal;

    public int TickDeltasCount => _tickDeltasCount;
    //public int TickDeltasTotal => _tickDeltasTotal;
    //public int AverageTickDelta => _tickDeltasCount == 0 ? 0 : (_tickDeltasTotal / _tickDeltasCount);
    public int TickDeltasTotal => _tickDeltasTotal == 0 ? _baseline.GetValueOrDefault() : _tickDeltasTotal;
    public int AverageTickDelta => _tickDeltasCount == 0 ? _baseline.GetValueOrDefault() : (_tickDeltasTotal / _tickDeltasCount);

    public TimeConsistencyChecker(bool useLastTimeAsBaseline = false)
    {
        _useLastTimeAsBaseline = useLastTimeAsBaseline;
    }

    public void Reset()
    {
        _lastTickCount = null;
        _baseline = null;

        _tickDeltasCount = 0;
        _tickDeltasTotal = 0;
    }

    /// <summary>
    /// Updates the stored tick counts and returns whether the timing is consistent.
    /// </summary>
    /// <returns>
    /// <typeparamref name="true"/> if consistent, otherwise <typeparamref name="false"/>.
    /// </returns>
    public bool UpdateAndCheckIfConsistent()
    {
        int curTickCount = Environment.TickCount;
        bool result = IsConsistent(in curTickCount);
        if (!result)
        {
            Reset();
        }
        _lastTickCount = curTickCount;
        return result;
    }

    private bool IsConsistent(in int curTickCount)
    {
        if (_lastTickCount is null)
        {
            return true;
        }

        int curTickDelta = curTickCount - _lastTickCount.Value;

        if (curTickDelta > MaxTimeDelta)
        {
            return false;
        }

        ++_tickDeltasCount;
        _tickDeltasTotal += curTickDelta;

        if (_baseline is null)
        {
            _baseline = curTickDelta;
            return true;
        }

        int baseline = _baseline.Value;
        if (_useLastTimeAsBaseline)
        {
            _baseline = curTickDelta;
        }

        return curTickDelta > baseline - ConsistencyMargin && curTickDelta < baseline + ConsistencyMargin;
    }
}
