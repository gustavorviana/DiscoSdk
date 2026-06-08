using DiscoSdk.Caching;

namespace DiscoSdk.Hosting.Caching;

/// <summary>
/// DI carrier for the configured <see cref="StickerCacheFlag"/> bitfield. Wrapped in a record so
/// the SDK can distinguish "host did not register a value" from "host explicitly registered
/// <see cref="StickerCacheFlag.None"/>".
/// </summary>
internal sealed record StickerCacheConfiguration(StickerCacheFlag Flags);
