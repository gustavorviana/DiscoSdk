using DiscoSdk.Caching;

namespace DiscoSdk.Hosting.Caching;

/// <summary>
/// DI carrier for the configured <see cref="PresenceCacheFlag"/> bitfield. Wrapped in a class
/// so the SDK can distinguish "host did not register a value" from "host explicitly registered
/// <see cref="PresenceCacheFlag.None"/>".
/// </summary>
internal sealed record PresenceCacheConfiguration(PresenceCacheFlag Flags);
