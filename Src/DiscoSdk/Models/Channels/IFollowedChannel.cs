namespace DiscoSdk.Models.Channels;

/// <summary>
/// Read-only response for <c>POST /channels/{channel.id}/followers</c> — the announcement channel
/// has been hooked up to the target channel via a Discord-managed webhook.
/// </summary>
public interface IFollowedChannel
{
	/// <summary>Id of the target channel where forwarded messages will arrive.</summary>
	Snowflake ChannelId { get; }

	/// <summary>Id of the webhook Discord created to forward the announcements.</summary>
	Snowflake WebhookId { get; }
}
