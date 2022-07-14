using System;

namespace ActionRepeater.Core.Utilities;

public class TimeConsistencyChecker
{
    public int TickDeltasCount => _tickDeltasCount;

    public int TickDeltasTotal => _tickDeltasTotal == 0 ? _baseline.GetValueOrDefault() : _tickDeltasTotal;

    public int AverageTickDelta => _tickDeltasCount == 0 ? _baseline.GetValueOrDefault() : (_tickDeltasTotal / _tickDeltasCount);

    private readonly int _maxTimeDelta;
    private readonly int _consistencyMargin;

    private readonly bool _useLastTimeAsBaseline;

    private int? _lastTickCount;
    private int? _baseline;

    private int _tickDeltasCount;
    private int _tickDeltasTotal;

    public TimeConsistencyChecker(bool useLastTimeAsBaseline = false, int maxTimeDelta = 1000, int consistencyMargin = 150)
    {
        _useLastTimeAsBaseline = useLastTimeAsBaseline;
        _maxTimeDelta = maxTimeDelta;
        _consistencyMargin = consistencyMargin;
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
        bool isConsistent = IsConsistent(curTickCount);
        if (!isConsistent)
        {
            Reset();
        }
        _lastTickCount = curTickCount;
        return isConsistent;
    }

    private bool IsConsistent(int curTickCount)
    {
        if (_lastTickCount is null)
        {
            return true;
        }

        int curTickDelta = curTickCount - _lastTickCount.Value;

        if (curTickDelta > _maxTimeDelta)
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

        return curTickDelta > baseline - _consistencyMargin && curTickDelta < baseline + _consistencyMargin;
    }
}
