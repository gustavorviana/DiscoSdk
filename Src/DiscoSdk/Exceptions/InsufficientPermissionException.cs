using DiscoSdk.Rest;
using System.Net;

namespace DiscoSdk.Exceptions;

/// <summary>
/// Exception thrown when an operation requires a permission that the bot does not have.
/// </summary>
/// <remarks>
/// Inherits from <see cref="DiscordApiException"/> so generic <c>catch (DiscordApiException)</c>
/// handlers continue to fire. The SDK raises this specific subtype in two situations:
/// <list type="bullet">
/// <item>
/// The Discord REST API responds with HTTP <c>403 Forbidden</c> and a Discord error code that
/// indicates a permission failure (<c>50001 Missing Access</c> or <c>50013 Missing Permissions</c>).
/// The <see cref="DiscordApiException.DiscordCode"/> and <see cref="DiscordApiException.DiscordMessage"/>
/// fields carry the original payload; <see cref="RequiredPermission"/> is <c>null</c> because
/// Discord does not name the specific permission in the response.
/// </item>
/// <item>
/// SDK-side preflight checks that know the required permission name (for example, when an
/// operation has additional client-side constraints beyond the simple REST call) construct
/// the exception with the <see cref="RequiredPermission"/> populated and a synthetic
/// <see cref="HttpStatusCode.Forbidden"/> status.
/// </item>
/// </list>
/// </remarks>
public class InsufficientPermissionException : DiscordApiException
{
	/// <summary>
	/// Gets the name of the required Discord permission, when known. <c>null</c> for exceptions
	/// raised from a Discord <c>50001</c>/<c>50013</c> response (Discord does not name the
	/// specific permission in the payload).
	/// </summary>
	public string? RequiredPermission { get; }

	/// <summary>
	/// Initializes a new instance from a Discord REST response that returned a permission error
	/// (HTTP 403 with code 50001 or 50013).
	/// </summary>
	public InsufficientPermissionException(HttpStatusCode statusCode, string? httpReasonPhrase, DiscordApiError error)
		: base(statusCode, httpReasonPhrase, error)
	{
		RequiredPermission = null;
	}

	/// <summary>
	/// Initializes a new instance for SDK-side preflight checks that already know the required
	/// permission name. Synthesizes <see cref="HttpStatusCode.Forbidden"/> so consumers that
	/// branch on <see cref="DiscordApiException.StatusCode"/> see a consistent value.
	/// </summary>
	/// <param name="message">Human-readable error message.</param>
	/// <param name="requiredPermission">Discord permission name (e.g. <c>"MANAGE_MESSAGES"</c>).</param>
	public InsufficientPermissionException(string message, string requiredPermission)
		: base(HttpStatusCode.Forbidden, message, new DiscordApiError { Message = message, Code = 50013 })
	{
		RequiredPermission = requiredPermission;
	}

	/// <summary>
	/// Creates an <see cref="InsufficientPermissionException"/> for a specific operation that requires a permission.
	/// </summary>
	/// <param name="requiredPermission">The permission that is required.</param>
	/// <param name="operation">The operation that was attempted.</param>
	/// <returns>A new instance of <see cref="InsufficientPermissionException"/>.</returns>
	public static InsufficientPermissionException Operation(string requiredPermission, string operation)
	{
		return new InsufficientPermissionException($"Cannot {operation}. The bot requires the {requiredPermission} permission.", requiredPermission);
	}
}
