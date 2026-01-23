namespace DiscoSdk.Models.Messages;

public class MessageFile
{
    public string FileName { get; }
    public string? Description { get; }
    public byte[] Buffer { get; }
    public string? ContentType { get; }

    public MessageFile(string fileName, string? description, byte[] buffer, string? contentType = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

        ArgumentNullException.ThrowIfNull(buffer);

        Buffer = buffer;
        FileName = fileName;
        Description = description;
        ContentType = contentType;
    }
}