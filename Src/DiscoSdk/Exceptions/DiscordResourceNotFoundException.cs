using DiscoSdk.Rest;
using System.Net;

namespace DiscoSdk.Exceptions;

/// <summary>
/// Exception thrown when the Discord REST API responds with HTTP <c>404 Not Found</c> and a
/// Discord JSON error code from the "Unknown X" family (codes <c>10001</c> through <c>10068</c>):
/// Unknown Account, Application, Channel, Guild, Integration, Invite, Member, Message,
/// Permission Overwrite, Role, Token, User, Emoji, Webhook, Thread, etc.
/// </summary>
/// <remarks>
/// Inherits from <see cref="DiscordApiException"/>, so generic <c>catch (DiscordApiException)</c>
/// handlers continue to fire. Use a specific <c>catch (DiscordResourceNotFoundException)</c>
/// when you want a different UX for "the resource was deleted / never existed / I have the wrong
/// id" without inspecting <see cref="DiscordApiException.DiscordCode"/> manually.
/// <para>
/// Source: Discord JSON Error Codes —
/// <see href="https://discord.com/developers/docs/topics/opcodes-and-status-codes#json"/>.
/// </para>
/// </remarks>
public class DiscordResourceNotFoundException : DiscordApiException
{
	internal DiscordResourceNotFoundException(HttpStatusCode statusCode, string? httpReasonPhrase, DiscordApiError error)
		: base(statusCode, httpReasonPhrase, error)
	{
	}
}
