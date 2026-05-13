using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Applications;

/// <summary>
/// The current user's role-connection data for a specific application — a connection-name/username pair
/// plus a free-form metadata dictionary keyed by the application's metadata records' keys.
/// Also implements its public read surface <see cref="IApplicationRoleConnection"/>.
/// </summary>
public class ApplicationRoleConnection : IApplicationRoleConnection
{
	/// <summary>The vanity name of the platform a bot has connected (max 50 chars).</summary>
	[JsonPropertyName("platform_name")]
	public string? PlatformName { get; set; }

	/// <summary>The username on the platform a bot has connected (max 100 chars).</summary>
	[JsonPropertyName("platform_username")]
	public string? PlatformUsername { get; set; }

	/// <summary>
	/// The metadata keys and values, keyed by the application metadata record's <c>key</c>. Each value is
	/// a string Discord parses according to its declared metadata type (boolean, integer or datetime ISO 8601).
	/// </summary>
	[JsonPropertyName("metadata")]
	public Dictionary<string, string>? Metadata { get; set; }

	IReadOnlyDictionary<string, string>? IApplicationRoleConnection.Metadata => Metadata;
}
