using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord guild category channel.
/// </summary>
public interface IGuildCategoryChannel : IGuildChannel
{
	/// <summary>
	/// Creates a builder for editing this channel.
	/// </summary>
	/// <returns>An <see cref="IEditGuildCategoryChannelRestAction"/> instance for editing the channel.</returns>
	IEditGuildCategoryChannelRestAction Edit();
}

