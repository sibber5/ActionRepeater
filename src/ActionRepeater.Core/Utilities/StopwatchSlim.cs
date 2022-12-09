using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ActionRepeater.Core.Utilities;

public sealed class StopwatchSlim
{
    private static readonly double _tickFrequencyMS = 1000.0 / Stopwatch.Frequency;
    private static readonly double _tickFrequencyNS = 1_000_000_000.0 / Stopwatch.Frequency;

    public long ElapsedMilliseconds => QPCTicksToMilliseconds(ElapsedTicks);

    public long ElapsedNanoseconds => QPCTicksToNanoseconds(ElapsedTicks);

    public long ElapsedTicks
    {
        get
        {
            if (_startTimestamp == null) throw new InvalidOperationException($"{nameof(StopwatchSlim)} was not started.");

            if (_stopTimestamp == null) return Stopwatch.GetTimestamp() - _startTimestamp.Value;

            return _stopTimestamp.Value - _startTimestamp.Value;
        }
    }

    public bool IsRunning => _stopTimestamp == null && _startTimestamp != null;

    private long? _startTimestamp;
    private long? _stopTimestamp;

    public void Start()
    {
        if (_startTimestamp != null || _stopTimestamp != null) throw new InvalidOperationException($"{nameof(StopwatchSlim)} is not reset or is still running.");

        _startTimestamp = Stopwatch.GetTimestamp();
    }

    public void Stop()
    {
        if (!IsRunning) throw new InvalidOperationException($"{nameof(StopwatchSlim)} is not running.");

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

    public long RestartAndGetElapsedMS() => QPCTicksToMilliseconds(RestartAndGetElapsedTicks());

    public long RestartAndGetElapsedNS() => QPCTicksToNanoseconds(RestartAndGetElapsedTicks());

    public long RestartAndGetElapsedTicks()
    {
        long currentTimestamp = Stopwatch.GetTimestamp();

        long elapsedTicks = _startTimestamp == null ? 0 : currentTimestamp - _startTimestamp.Value;

        _stopTimestamp = null;
        _startTimestamp = currentTimestamp;

        return elapsedTicks;
    }

    /// <summary>
    /// Converts QueryPerformanceCounter ticks to milliseconds.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long QPCTicksToMilliseconds(long ticks) => unchecked((long)(ticks * _tickFrequencyMS));

    /// <summary>
    /// Converts QueryPerformanceCounter ticks to milliseconds.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long QPCTicksToNanoseconds(long ticks) => unchecked((long)(ticks * _tickFrequencyNS));
}
