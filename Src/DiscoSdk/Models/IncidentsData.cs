using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

public sealed class IncidentsData
{
    /// <summary>
    /// Timestamp until which invites are disabled.
    /// </summary>
    [JsonPropertyName("invites_disabled_until")]
    public DateTimeOffset? InvitesDisabledUntil { get; set; }

    /// <summary>
    /// Timestamp until which DMs are disabled.
    /// </summary>
    [JsonPropertyName("dms_disabled_until")]
    public DateTimeOffset? DmsDisabledUntil { get; set; }
}
