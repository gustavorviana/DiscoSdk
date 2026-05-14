using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Inline file attachment displayed as a download card inside a Components V2 layout. The
/// <see cref="File"/> URL <strong>must</strong> use the <c>attachment://&lt;filename&gt;</c>
/// scheme referring to a file uploaded in the same multipart request — Discord does not accept
/// arbitrary HTTPS URLs here. Reference:
/// https://discord.com/developers/docs/components/reference#file
/// </summary>
public sealed class FileComponent : IMessageComponent
{
    /// <inheritdoc />
    [JsonPropertyName("type")]
    public ComponentType Type { get; set; } = ComponentType.File;

    /// <inheritdoc />
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Id { get; set; }

    /// <summary>Reference to the attached file — must use <c>attachment://...</c>.</summary>
    [JsonPropertyName("file")]
    public UnfurledMediaItem File { get; set; } = default!;

    /// <summary>If <c>true</c>, the file is hidden behind a spoiler overlay.</summary>
    [JsonPropertyName("spoiler")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Spoiler { get; set; }

    /// <summary>Filename. Set by Discord on read.</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>File size in bytes. Set by Discord on read.</summary>
    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Size { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public string? CustomId { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public bool? Disabled { get; set; }
}
