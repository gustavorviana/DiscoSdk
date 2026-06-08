namespace DiscoSdk.Models.Activities;

/// <summary>
/// Read-only view of <see cref="ActivitySecrets"/> — opaque handshake tokens Discord uses for
/// join / spectate / match flows. The SDK does not interpret these values; expose them as-is for
/// consumers that integrate with the Discord Rich Presence handshake.
/// </summary>
public interface IActivitySecrets
{
    /// <summary>Secret used by a client to request joining the activity, or <c>null</c>.</summary>
    string? Join { get; }

    /// <summary>Secret used by a client to spectate the activity, or <c>null</c>.</summary>
    string? Spectate { get; }

    /// <summary>Secret tied to a specific instanced match, or <c>null</c>.</summary>
    string? Match { get; }
}
