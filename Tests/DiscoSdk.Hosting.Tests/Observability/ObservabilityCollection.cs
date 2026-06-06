namespace DiscoSdk.Hosting.Tests.Observability;

/// <summary>
/// Forces every observability test into a single sequentially-run xUnit collection. The SDK's
/// <see cref="System.Diagnostics.Metrics.Meter"/> is a process-wide singleton, so a parallel
/// test recording a measurement against the same instrument leaks into another test's
/// <see cref="MeterListenerCapture"/> and breaks tag assertions. Tests that subscribe to the
/// SDK meter must all carry the matching <c>[Collection("Observability")]</c> attribute.
/// </summary>
[CollectionDefinition("Observability", DisableParallelization = true)]
public sealed class ObservabilityCollection;
