using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

public sealed class ActivityInstance
{
    /// <summary>
    /// The application id of the embedded activity.
    /// </summary>
    [JsonPropertyName("application_id")]
    public Snowflake ApplicationId { get; set; }

    /// <summary>
    /// The channel where this activity instance is running.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Snowflake ChannelId { get; set; }

    /// <summary>
    /// The unique id of this activity instance.
    /// </summary>
    [JsonPropertyName("instance_id")]
    public string InstanceId { get; set; } = default!;

    /// <summary>
    /// The launch id for this activity instance.
    /// </summary>
    [JsonPropertyName("launch_id")]
    public string? LaunchId { get; set; }
}
