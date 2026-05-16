namespace DiscoSdk.Models;

/// <summary>Read-only vanity URL snapshot for a guild — Discord's <c>/vanity-url</c> endpoint.</summary>
public interface IVanityUrl
{
	/// <summary>Vanity code (the path after <c>discord.gg/</c>), or <c>null</c> if not set.</summary>
	string? Code { get; }

	/// <summary>Number of times the vanity invite has been used.</summary>
	int Uses { get; }
}
