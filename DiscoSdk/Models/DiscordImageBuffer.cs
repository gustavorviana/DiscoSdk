namespace DiscoSdk.Models;

public sealed class DiscordImageBuffer(byte[] buffer, string? extension = null)
    : DiscordImage(string.IsNullOrEmpty(extension) ? GetImageExtension(buffer) : extension)
{
    public byte[] Buffer { get; } = buffer;

    public string ToBase64()
    {
        return Convert.ToBase64String(Buffer);
    }

    public static DiscordImageBuffer LoadFile(string filePath)
    {
        return new DiscordImageBuffer(File.ReadAllBytes(filePath));
    }

    public static DiscordImageBuffer? FromBase64(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        var bytes = Convert.FromBase64String(value);
        return new DiscordImageBuffer(bytes);
    }

    private static string GetImageExtension(byte[] buffer)
    {
        if (buffer.Length >= 2)
        {
            if (buffer[0] == 0xFF && buffer[1] == 0xD8)
                return "jpeg";
        }

        if (buffer.Length >= 3)
        {
            if (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46)
                return "gif";
        }

        if (buffer.Length >= 8)
        {
            if (buffer[0] == 0x89 &&
                buffer[1] == 0x50 &&
                buffer[2] == 0x4E &&
                buffer[3] == 0x47 &&
                buffer[4] == 0x0D &&
                buffer[5] == 0x0A &&
                buffer[6] == 0x1A &&
                buffer[7] == 0x0A)
                return "png";
        }

        throw new InvalidOperationException("Unsupported image format. Only PNG, JPEG and GIF are allowed.");
    }
}