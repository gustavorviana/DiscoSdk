namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord entity that has a unique identifier and a creation date.
/// </summary>
public interface IWithDiscordIdentity
{
	/// <summary>
	/// Gets the unique identifier of the entity.
	/// </summary>
	DiscordId Id { get; }

	/// <summary>
	/// Gets the date and time when this entity was created.
	/// </summary>
	DateTimeOffset CreatedAt { get; }
}