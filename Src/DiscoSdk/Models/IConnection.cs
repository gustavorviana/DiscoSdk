using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models;

/// <summary>
/// A Discord user connection — an account from another service the user has linked.
/// </summary>
public interface IConnection
{
	/// <summary>The ID of the connection (external service's ID).</summary>
	string Id { get; }

	/// <summary>The username of the connection account on the external service.</summary>
	string Name { get; }

	/// <summary>The service this connection points at (e.g. <c>twitch</c>, <c>youtube</c>).</summary>
	string Type { get; }

	/// <summary>Whether the connection has been revoked.</summary>
	bool? Revoked { get; }

	/// <summary>Partial server integrations this connection is tied to, if available.</summary>
	IReadOnlyList<IIntegration>? Integrations { get; }

	/// <summary>Whether the connection is verified.</summary>
	bool Verified { get; }

	/// <summary>Whether friend sync is enabled for this connection.</summary>
	bool FriendSync { get; }

	/// <summary>Whether activities related to this connection will be shown in presence updates.</summary>
	bool ShowActivity { get; }

	/// <summary>Whether the connection has a corresponding third-party OAuth2 token.</summary>
	bool TwoWayLink { get; }

	/// <summary>The visibility of the connection.</summary>
	ConnectionVisibility Visibility { get; }
}
