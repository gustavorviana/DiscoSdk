using System.Text.Json.Serialization;

namespace DiscoSdk.Rest.Actions.Messages;

public sealed class PartialAttachmentPayload
{
    [JsonPropertyName("id")]
    public ulong? Id { get; set; }

    [JsonPropertyName("filename")]
    public string? FileName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
