using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ActionRepeater.Core.Utilities;

public sealed class StopwatchSlim
{
    private static readonly double _tickFrequencyMS = 1000.0 / Stopwatch.Frequency;

    public long ElapsedMilliseconds
    {
        get
        {
            if (_startTimestamp == null) throw new InvalidOperationException($"{nameof(StopwatchSlim)} was not started.");

            if (_stopTimestamp == null) return QPCTicksToMilliseconds(Stopwatch.GetTimestamp() - _startTimestamp.Value);

            return QPCTicksToMilliseconds(_stopTimestamp.Value - _startTimestamp.Value);
        }
    }

    private long? _startTimestamp = null;
    private long? _stopTimestamp = null;

    public void Start()
    {
        if (_startTimestamp != null || _stopTimestamp != null) throw new InvalidOperationException($"{nameof(StopwatchSlim)} is not reset or is still running.");

        _startTimestamp = Stopwatch.GetTimestamp();
    }

    public void Stop()
    {
        if (_stopTimestamp != null || _startTimestamp == null) throw new InvalidOperationException($"{nameof(StopwatchSlim)} is not running.");

        _stopTimestamp = Stopwatch.GetTimestamp();
    }

    public void Reset()
    {
        _stopTimestamp = null;
        _startTimestamp = null;
    }

    public void Restart()
    {
        _stopTimestamp = null;
        _startTimestamp = Stopwatch.GetTimestamp();
    }

    public long RestartAndGetElapsedMS()
    {
        long currentTimestamp = Stopwatch.GetTimestamp();

        long elapsedMS = _startTimestamp == null ? 0 : QPCTicksToMilliseconds(currentTimestamp - _startTimestamp.Value);

        _stopTimestamp = null;
        _startTimestamp = currentTimestamp;

        return elapsedMS;
    }

    /// <summary>
    /// Converts QueryPerformanceCounter ticks to milliseconds.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long QPCTicksToMilliseconds(long ticks) => unchecked((long)(ticks * _tickFrequencyMS));
}
