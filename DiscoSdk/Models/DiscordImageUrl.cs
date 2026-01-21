namespace DiscoSdk.Models;

public sealed class DiscordImageUrl(string url, string extension) : DiscordImage(extension)
{
    public string Url => url;

    public static DiscordImageUrl ParseAvatar(Snowflake userId, string? avatarHash)
    {
        if (avatarHash == null)
            return new DiscordImageUrl($"https://cdn.discordapp.com/embed/avatars/{userId % 5}.png", "png");

        var ext = GetHashExtension(avatarHash);
        return new DiscordImageUrl($"https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.{ext}", ext);
    }

    public static DiscordImageUrl? ParseBanner(Snowflake userId, string? bannerHash)
    {
        if (bannerHash == null)
            return null;

        var ext = GetHashExtension(bannerHash);
        return new DiscordImageUrl($"https://cdn.discordapp.com/banners/{userId}/{bannerHash}.{ext}", ext);
    }

    public static DiscordImageUrl? ParseIcon(Snowflake guildId, string? iconHash)
    {
        if (string.IsNullOrWhiteSpace(iconHash))
            return null;

        var ext = GetHashExtension(iconHash);
        return new DiscordImageUrl($"https://cdn.discordapp.com/icons/{guildId}/{iconHash}.{ext}", ext);
    }

    public static DiscordImageUrl? ParseSplash(Snowflake guildId, string? splashHash)
    {
        if (string.IsNullOrWhiteSpace(splashHash))
            return null;

        // Splash is always png/jpg? Discord CDN supports png/jpg/webp for static; gifs are not used for splash.
        // Keeping same rule for simplicity.
        var ext = GetHashExtension(splashHash);
        return new DiscordImageUrl($"https://cdn.discordapp.com/splashes/{guildId}/{splashHash}.{ext}", ext);
    }

    public static DiscordImageUrl? ParseDiscoverySplash(Snowflake guildId, string? discoverySplashHash)
    {
        if (string.IsNullOrWhiteSpace(discoverySplashHash))
            return null;

        var ext = GetHashExtension(discoverySplashHash);
        return new DiscordImageUrl($"https://cdn.discordapp.com/discovery-splashes/{guildId}/{discoverySplashHash}.{ext}", ext);
    }

    private static string GetHashExtension(string hash)
    {
        return hash.StartsWith("a_") ? "gif" : "png";
    }
}