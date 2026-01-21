using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Pools;

public class PollAnswer
{
    [JsonPropertyName("poll_media")]
    public required PollText PoolMedia { get; set; }
}