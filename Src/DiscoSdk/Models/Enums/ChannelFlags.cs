namespace DiscoSdk.Models.Enums;

/// <summary>
/// Represents channel flags.
/// </summary>
[Flags]
public enum ChannelFlags
{
    None = 0,
    Pinned = 1 << 1,
    RequireTag = 1 << 4,
    HideMediaDownloadOptions = 1 << 15
}