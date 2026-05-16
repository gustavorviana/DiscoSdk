using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Users;

/// <summary>
/// A Discord connection — an account from another service the user has linked (Steam, Twitch,
/// Twitter, etc.). Also implements <see cref="IConnection"/> as its public read surface.
/// </summary>
internal class Connection : IConnection
{
	/// <summary>The ID of the connection (external service's ID).</summary>
	[JsonPropertyName("id")]
	public string Id { get; set; } = default!;

	/// <summary>The username of the connection account on the external service.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>The service this connection points at (e.g. <c>twitch</c>, <c>youtube</c>).</summary>
	[JsonPropertyName("type")]
	public string Type { get; set; } = default!;

	/// <summary>Whether the connection has been revoked.</summary>
	[JsonPropertyName("revoked")]
	public bool? Revoked { get; set; }

	/// <summary>A list of partial server integrations this connection is tied to.</summary>
	[JsonPropertyName("integrations")]
	public Integration[]? Integrations { get; set; }

	/// <summary>Whether the connection is verified.</summary>
	[JsonPropertyName("verified")]
	public bool Verified { get; set; }

	/// <summary>Whether friend sync is enabled for this connection.</summary>
	[JsonPropertyName("friend_sync")]
	public bool FriendSync { get; set; }

	/// <summary>Whether activities related to this connection will be shown in presence updates.</summary>
	[JsonPropertyName("show_activity")]
	public bool ShowActivity { get; set; }

	/// <summary>Whether the connection has a corresponding third-party OAuth2 token.</summary>
	[JsonPropertyName("two_way_link")]
	public bool TwoWayLink { get; set; }

	/// <summary>The visibility of the connection.</summary>
	[JsonPropertyName("visibility")]
	public ConnectionVisibility Visibility { get; set; }

	IReadOnlyList<IIntegration>? IConnection.Integrations => null;
}
