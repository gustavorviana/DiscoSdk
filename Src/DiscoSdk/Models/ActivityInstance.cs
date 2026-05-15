using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// A running instance of an embedded Discord Activity. Returned by
/// <c>GET /applications/{application_id}/activity-instances/{instance_id}</c> and embedded
/// inside the <c>activity_instances</c> array on <c>GUILD_CREATE</c> dispatches.
/// </summary>
/// <remarks>
/// Source: <see href="https://discord.com/developers/docs/resources/application#get-application-activity-instance"/>.
/// </remarks>
public sealed class ActivityInstance
{
    /// <summary>Application id of the embedded activity.</summary>
    [JsonPropertyName("application_id")]
    public Snowflake ApplicationId { get; set; }

    /// <summary>Unique id of this activity instance.</summary>
    [JsonPropertyName("instance_id")]
    public string InstanceId { get; set; } = default!;

    /// <summary>Launch id for this activity instance.</summary>
    [JsonPropertyName("launch_id")]
    public string? LaunchId { get; set; }

    /// <summary>
    /// Location where the activity is running (channel + guild context, channel kind).
    /// Populated on the <c>Get Application Activity Instance</c> response.
    /// </summary>
    [JsonPropertyName("location")]
    public ActivityInstanceLocation? Location { get; set; }

    /// <summary>User ids currently connected to the activity instance.</summary>
    [JsonPropertyName("users")]
    public Snowflake[] Users { get; set; } = [];

    /// <summary>
    /// Legacy top-level <c>channel_id</c> field still emitted on some Discord payloads (notably
    /// the <c>activity_instances</c> array in <c>GUILD_CREATE</c>). For responses from
    /// <c>Get Application Activity Instance</c>, prefer
    /// <see cref="ActivityInstanceLocation.ChannelId"/> via <see cref="Location"/>.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Snowflake? ChannelId { get; set; }
}
