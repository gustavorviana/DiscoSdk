using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

public sealed class EmbeddedActivity
{
    /// <summary>
    /// The application id of the embedded activity.
    /// </summary>
    [JsonPropertyName("application_id")]
    public Snowflake ApplicationId { get; set; }

    /// <summary>
    /// The display name of the embedded activity.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
}
