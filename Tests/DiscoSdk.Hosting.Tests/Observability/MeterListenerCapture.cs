using DiscoSdk.Hosting.Observability;
using System.Diagnostics.Metrics;

namespace DiscoSdk.Hosting.Tests.Observability;

/// <summary>
/// Test helper that subscribes a <see cref="MeterListener"/> to the SDK's
/// <see cref="DiscoSdkDiagnostics.Meter"/> and captures every measurement against a filter set of
/// instrument names. Disposing the capture detaches the listener.
/// </summary>
/// <remarks>
/// Measurements are stored in two typed lists — one for <see cref="long"/> instruments
/// (counters) and one for <see cref="double"/> instruments (histograms). The listener is
/// instantiated on construction so it is live for the duration of the test scope.
/// </remarks>
internal sealed class MeterListenerCapture : IDisposable
{
    private readonly MeterListener _listener;
    private readonly HashSet<string> _instrumentFilter;
    private readonly object _gate = new();
    private readonly List<CapturedMeasurement<long>> _long = new();
    private readonly List<CapturedMeasurement<double>> _double = new();

    public MeterListenerCapture(params string[] instrumentNames)
    {
        _instrumentFilter = new HashSet<string>(instrumentNames, StringComparer.Ordinal);

        _listener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name != DiscoSdkDiagnostics.MeterName)
                    return;
                if (_instrumentFilter.Count > 0 && !_instrumentFilter.Contains(instrument.Name))
                    return;
                listener.EnableMeasurementEvents(instrument);
            },
        };
        _listener.SetMeasurementEventCallback<long>(OnLong);
        _listener.SetMeasurementEventCallback<double>(OnDouble);
        _listener.Start();
    }

    public IReadOnlyList<CapturedMeasurement<long>> Long
    {
        get { lock (_gate) return _long.ToList(); }
    }

    public IReadOnlyList<CapturedMeasurement<double>> Double
    {
        get { lock (_gate) return _double.ToList(); }
    }

    public IEnumerable<CapturedMeasurement<long>> LongFor(string instrumentName)
        => Long.Where(m => m.InstrumentName == instrumentName);

    public IEnumerable<CapturedMeasurement<double>> DoubleFor(string instrumentName)
        => Double.Where(m => m.InstrumentName == instrumentName);

    private void OnLong(Instrument instrument, long value, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
    {
        var snapshot = SnapshotTags(tags);
        lock (_gate)
            _long.Add(new CapturedMeasurement<long>(instrument.Name, value, snapshot));
    }

    private void OnDouble(Instrument instrument, double value, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
    {
        var snapshot = SnapshotTags(tags);
        lock (_gate)
            _double.Add(new CapturedMeasurement<double>(instrument.Name, value, snapshot));
    }

    private static IReadOnlyDictionary<string, object?> SnapshotTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var dict = new Dictionary<string, object?>(tags.Length, StringComparer.Ordinal);
        foreach (var tag in tags)
            dict[tag.Key] = tag.Value;
        return dict;
    }

    public void Dispose() => _listener.Dispose();
}

internal sealed record CapturedMeasurement<T>(string InstrumentName, T Value, IReadOnlyDictionary<string, object?> Tags)
{
    public object? Tag(string key) => Tags.TryGetValue(key, out var v) ? v : null;
}
