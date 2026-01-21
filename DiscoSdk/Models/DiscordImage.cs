namespace DiscoSdk.Models;

public abstract class DiscordImage(string extension)
{
    public string ImageType { get; } = "image/" + extension;
    public string Extension => extension;
}