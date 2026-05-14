using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Requests.Members;

/// <summary>
/// Payload for the <c>Request Guild Members</c> gateway command (op 8). Sent on the main gateway
/// to ask Discord to stream the guild's member list back as <c>GUILD_MEMBERS_CHUNK</c> events.
/// Reference: https://discord.com/developers/docs/topics/gateway-events#request-guild-members
/// </summary>
internal class RequestGuildMembersRequest
{
	/// <summary>The guild whose members are being requested.</summary>
	[JsonPropertyName("guild_id")]
	public string GuildId { get; set; } = default!;

	/// <summary>
	/// Username prefix filter. Use <see cref="string.Empty"/> with <see cref="Limit"/> = 0 to fetch
	/// the entire member list (requires <c>GUILD_MEMBERS</c> privileged intent). Mutually exclusive
	/// with <see cref="UserIds"/>.
	/// </summary>
	[JsonPropertyName("query")]
	public string? Query { get; set; }

	/// <summary>
	/// Cap on returned members. 0 means no cap (used together with empty <see cref="Query"/> to
	/// request all members).
	/// </summary>
	[JsonPropertyName("limit")]
	public int Limit { get; set; }

	/// <summary>Whether to include presence data with the response chunks.</summary>
	[JsonPropertyName("presences")]
	public bool? Presences { get; set; }

	/// <summary>
	/// Specific user ids to fetch (max 100). Mutually exclusive with <see cref="Query"/>.
	/// </summary>
	[JsonPropertyName("user_ids")]
	public string[]? UserIds { get; set; }

	/// <summary>
	/// Correlation nonce (max 32 chars). Echoed back on every <c>GUILD_MEMBERS_CHUNK</c> so the
	/// SDK can match chunks to the originating request.
	/// </summary>
	[JsonPropertyName("nonce")]
	public string Nonce { get; set; } = default!;
}
