using DiscoSdk.Models;

namespace DiscoSdk;

public interface IWebhookInfo
{
    Snowflake Id { get; }
    string Name { get; }
    string? Avatar { get; }
    Snowflake? ChannelId { get; }
    Snowflake? GuildId { get; }
    string? ApplicationId { get; }
}