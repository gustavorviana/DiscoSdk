using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a category channel in a Discord guild — a positionable container that groups other
/// channels but holds no content of its own.
/// </summary>
public interface IGuildCategoryChannel : IStandardGuildChannel
{
	/// <summary>
	/// Gets the channels that belong to this category.
	/// </summary>
	/// <returns>A REST action resolving to the channels nested under this category.</returns>
	IRestAction<IReadOnlyList<IGuildChannel>> GetChannels();
}
