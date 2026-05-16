using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <inheritdoc cref="IWelcomeScreen"/>
internal class WelcomeScreen : IWelcomeScreen
{
	[JsonPropertyName("description")]
	public string? Description { get; init; }

	[JsonPropertyName("welcome_channels")]
	public WelcomeScreenChannel[]? WelcomeChannels { get; init; }

	IReadOnlyList<IWelcomeScreenChannel>? IWelcomeScreen.WelcomeChannels => WelcomeChannels;
}
