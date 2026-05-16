using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <inheritdoc cref="IIncidentsData"/>
internal sealed class IncidentsData : IIncidentsData
{
    [JsonPropertyName("invites_disabled_until")]
    public DateTimeOffset? InvitesDisabledUntil { get; init; }

    [JsonPropertyName("dms_disabled_until")]
    public DateTimeOffset? DmsDisabledUntil { get; init; }
}
