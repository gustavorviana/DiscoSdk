namespace DiscoSdk.Models.Requests.Messages;

internal sealed record MessageAttachmentMetadata(
    object Id,
    string? FileName = null,
    string? Description = null
);