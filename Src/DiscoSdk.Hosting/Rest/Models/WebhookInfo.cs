using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Rest.Models;

internal class WebhookInfo : IWebhookInfo
{
    public Snowflake Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Avatar { get; set; }

    public Snowflake? ChannelId { get; set; }

    public Snowflake? GuildId { get; set; }

    public string? ApplicationId { get; set; }
}
