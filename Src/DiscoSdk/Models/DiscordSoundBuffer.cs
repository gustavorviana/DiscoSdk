namespace DiscoSdk.Models;

/// <summary>
/// In-memory audio payload sent with <c>POST /guilds/:id/soundboard-sounds</c>. Discord accepts
/// MP3 (<c>audio/mpeg</c>) and OGG (<c>audio/ogg</c>) clips of at most 512 KiB and at most
/// 5.2 seconds — the SDK encodes the bytes as a base64 data URI at upload time.
/// </summary>
/// <remarks>
/// The codec is detected from the magic header so callers don't have to spell it out. Validation
/// of duration is not performed locally (would require codec parsing); Discord returns an HTTP 400
/// when the clip exceeds the limit.
/// </remarks>
public sealed class DiscordSoundBuffer
{
    private const string Mp3Mime = "audio/mpeg";
    private const string OggMime = "audio/ogg";

    /// <summary>Discord's hard byte cap on a single soundboard upload.</summary>
    public const int MaxSizeBytes = 512 * 1024;

    /// <summary>The raw audio bytes (MP3 or OGG container).</summary>
    public byte[] Buffer { get; }

    /// <summary>
    /// MIME type of the buffer — one of <c>audio/mpeg</c> or <c>audio/ogg</c>. Discord rejects
    /// other formats with a 400.
    /// </summary>
    public string MimeType { get; }

    /// <summary>
    /// Creates the buffer from raw bytes. When <paramref name="mimeType"/> is <c>null</c> the
    /// codec is detected from the buffer's magic header; throws if neither MP3 nor OGG.
    /// </summary>
    public DiscordSoundBuffer(byte[] buffer, string? mimeType = null)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (buffer.Length == 0)
            throw new ArgumentException("Sound buffer cannot be empty.", nameof(buffer));
        if (buffer.Length > MaxSizeBytes)
            throw new ArgumentException($"Sound buffer exceeds Discord's {MaxSizeBytes / 1024} KiB cap.", nameof(buffer));

        Buffer = buffer;
        MimeType = string.IsNullOrEmpty(mimeType) ? DetectMimeType(buffer) : mimeType;
    }

    /// <summary>Reads the file at <paramref name="filePath"/> into a new <see cref="DiscordSoundBuffer"/>.</summary>
    public static DiscordSoundBuffer LoadFile(string filePath)
        => new(File.ReadAllBytes(filePath));

    /// <summary>Base64 encoding of the audio bytes — used by the data-URI payload.</summary>
    public string ToBase64() => Convert.ToBase64String(Buffer);

    /// <summary>
    /// Returns the <c>data:audio/...;base64,...</c> URI Discord expects in the <c>sound</c>
    /// field of the create-sound payload.
    /// </summary>
    public string ToDataUri() => $"data:{MimeType};base64,{ToBase64()}";

    private static string DetectMimeType(byte[] buffer)
    {
        // OGG container: "OggS" at offset 0.
        if (buffer.Length >= 4 && buffer[0] == 0x4F && buffer[1] == 0x67 && buffer[2] == 0x67 && buffer[3] == 0x53)
            return OggMime;

        // MP3: either ID3v2 ("ID3" at offset 0) or a raw MPEG audio sync word (0xFF 0xFB / 0xFA / 0xF3 / 0xF2).
        if (buffer.Length >= 3 && buffer[0] == 0x49 && buffer[1] == 0x44 && buffer[2] == 0x33)
            return Mp3Mime;
        if (buffer.Length >= 2 && buffer[0] == 0xFF && (buffer[1] & 0xE0) == 0xE0)
            return Mp3Mime;

        throw new InvalidOperationException(
            "Unsupported audio format. Soundboard sounds must be MP3 or OGG; pass an explicit MIME type if detection fails.");
    }
}
