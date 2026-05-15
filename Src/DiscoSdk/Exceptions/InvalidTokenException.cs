using DiscoSdk.Rest;
using System.Net;

namespace DiscoSdk.Exceptions;

/// <summary>
/// Exception thrown when Discord rejects the bot token — HTTP <c>401 Unauthorized</c> with
/// Discord JSON error code <c>40001</c> (<c>Unauthorized</c>) or <c>50014</c>
/// (<c>Invalid authentication token provided</c>).
/// </summary>
/// <remarks>
/// Distinct from <see cref="InsufficientPermissionException"/>: this signals the credential
/// itself is wrong (revoked, regenerated, mistyped, or the application is suspended), not that
/// the bot lacks a Discord permission for the action. The corrective action is also different —
/// rotate the token, do not retry.
/// <para>
/// Source: Discord JSON Error Codes —
/// <see href="https://discord.com/developers/docs/topics/opcodes-and-status-codes#json"/>.
/// </para>
/// </remarks>
public class InvalidTokenException : DiscordApiException
{
	internal InvalidTokenException(HttpStatusCode statusCode, string? httpReasonPhrase, DiscordApiError error)
		: base(statusCode, httpReasonPhrase, error)
	{
	}
}
