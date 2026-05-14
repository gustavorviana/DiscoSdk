using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk;

/// <summary>
/// Public surface for application-owned (global) emoji operations. Exposed via
/// <see cref="IDiscordClient.ApplicationEmojis"/>.
/// </summary>
public interface IApplicationEmojis
{
    /// <summary>Lists the application-owned emojis.</summary>
    IRestAction<IReadOnlyList<IEmoji>> List();

    /// <summary>Gets a single application-owned emoji by id.</summary>
    IRestAction<IEmoji> Get(Snowflake emojiId);

    /// <summary>Creates a new application-owned emoji from <paramref name="image"/>.</summary>
    /// <param name="name">Emoji name (2-32 chars).</param>
    /// <param name="image">Image buffer (PNG/JPEG/GIF, 128x128).</param>
    IRestAction<IEmoji> Create(string name, DiscordImageBuffer image);
}
