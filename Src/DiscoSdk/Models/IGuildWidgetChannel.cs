namespace DiscoSdk.Models;

/// <summary>
/// A channel entry inside an <see cref="IGuildWidget"/>.
/// </summary>
public interface IGuildWidgetChannel
{
	/// <summary>The channel id.</summary>
	Snowflake Id { get; }

	/// <summary>The channel name.</summary>
	string Name { get; }

	/// <summary>The channel's sort position.</summary>
	int Position { get; }
}
