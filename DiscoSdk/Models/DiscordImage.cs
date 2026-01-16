using System.Net.Mime;

namespace DiscoSdk.Models;

public class DiscordImage(byte[] buffer, string? type = null)
{
    public byte[] Buffer => buffer;

    public string ImageType { get; } = type ?? GetImageType(buffer);

    public string ToBase64()
    {
        return Convert.ToBase64String(Buffer);
    }

    public static DiscordImage LoadFile(string filePath)
    {
        return new DiscordImage(File.ReadAllBytes(filePath));
    }

    public static DiscordImage? FromBase64(string? value)
    {
        if (string.IsNullOrEmpty(value))
             return null;

        var bytes = Convert.FromBase64String(value);
        return new DiscordImage(bytes);
    }

    private static string GetImageType(byte[] buffer)
    {
        if (buffer.Length >= 2)
        {
            // JPEG: FF D8
            if (buffer[0] == 0xFF && buffer[1] == 0xD8)
                return MediaTypeNames.Image.Jpeg;
        }

        if (buffer.Length >= 3)
        {
            // GIF: "GIF"
            if (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46)
                return MediaTypeNames.Image.Gif;
        }

        if (buffer.Length >= 8)
        {
            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (buffer[0] == 0x89 &&
                buffer[1] == 0x50 &&
                buffer[2] == 0x4E &&
                buffer[3] == 0x47 &&
                buffer[4] == 0x0D &&
                buffer[5] == 0x0A &&
                buffer[6] == 0x1A &&
                buffer[7] == 0x0A)
                return MediaTypeNames.Image.Png;
        }

        throw new InvalidOperationException("Unsupported image format. Only PNG, JPEG and GIF are allowed.");
    }
}