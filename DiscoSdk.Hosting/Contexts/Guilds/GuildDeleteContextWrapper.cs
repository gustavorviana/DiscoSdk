using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;
using System.Text.Json;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildDeleteContextWrapper(DiscordClient client, JsonElement payload) : ContextWrapper(client), IGuildDeleteContext
{
    public Snowflake Id { get; }
        = Snowflake.TryParse(payload.GetProperty("id").GetString(), out var snowflake) ? snowflake : default;

    public bool Unavailable { get; } 
        = payload.TryGetProperty("unavailable", out var unavailable) && unavailable.GetBoolean();
}
