using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

public interface IGuildDeleteContext : IContext
{
    /// <summary>
    /// Gets or sets the ID of the guild that was deleted or became unavailable.
    /// </summary>
    Snowflake Id { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the guild is unavailable.
    /// </summary>
    bool Unavailable { get; }
}