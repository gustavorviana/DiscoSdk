using DiscoSdk.Rest;
using System.Net;

namespace DiscoSdk.Exceptions;

/// <summary>
/// Exception thrown when Discord rejects the request payload — HTTP <c>400 Bad Request</c> with
/// Discord JSON error code <c>50035</c> (<c>Invalid Form Body</c>). Surfaces the structured
/// per-field validation errors via the inherited <see cref="DiscordApiException.ValidationErrors"/>
/// collection so callers can format and log them without re-parsing the response.
/// </summary>
/// <remarks>
/// Common during development when a request body misses a required field, exceeds a length,
/// passes an unknown enum value, etc. Each entry in <see cref="DiscordApiException.ValidationErrors"/>
/// names the offending field and carries Discord's per-field error code + message (and, for
/// bulk requests, the list index).
/// <para>
/// Source: Discord JSON Error Codes —
/// <see href="https://discord.com/developers/docs/topics/opcodes-and-status-codes#json"/>.
/// </para>
/// </remarks>
public class InvalidRequestBodyException : DiscordApiException
{
	internal InvalidRequestBodyException(HttpStatusCode statusCode, string? httpReasonPhrase, DiscordApiError error)
		: base(statusCode, httpReasonPhrase, error)
	{
	}
}
