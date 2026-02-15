using DiscoSdk.Rest;
using System.Net;

namespace DiscoSdk.Exceptions
{
    /// <summary>
    /// Exception thrown when a Discord REST API request returns a non-success HTTP status code.
    /// </summary>
    /// <remarks>
    /// The exception message is derived from the Discord error payload when available; otherwise, it falls back
    /// to the HTTP status code and reason phrase.
    /// </remarks>
    public sealed class DiscordApiException(HttpStatusCode statusCode, string? httpReasonPhrase, DiscordApiError? error)
        : DiscoException(GetValidMessage(statusCode, httpReasonPhrase, error))
    {
        /// <summary>
        /// Gets the HTTP status code returned by the Discord API.
        /// </summary>
        public HttpStatusCode StatusCode => statusCode;

        /// <summary>
        /// Gets the HTTP reason phrase returned by the server, when available (e.g. "Bad Request").
        /// </summary>
        public string? HttpReasonPhrase => httpReasonPhrase;

        /// <summary>
        /// Gets the Discord-specific error code from the response body, when available.
        /// </summary>
        /// <remarks>
        /// This value corresponds to Discord's JSON <c>code</c> field and is distinct from the HTTP status code.
        /// </remarks>
        public int? DiscordCode => error?.Code;

        /// <summary>
        /// Gets the top-level error message from the response body, when available.
        /// </summary>
        /// <remarks>
        /// This value corresponds to Discord's JSON <c>message</c> field.
        /// </remarks>
        public string? DiscordMessage => error?.Message;

        /// <summary>
        /// Gets the normalized validation errors returned by Discord, if any.
        /// </summary>
        /// <remarks>
        /// This collection is typically populated for HTTP 400 responses such as "Invalid Form Body" (Discord code 50035).
        /// Each entry may include an optional list index (for bulk requests), a field name, and either structured
        /// field errors (<c>code</c>/<c>message</c>) and/or plain string messages.
        /// </remarks>
        public IReadOnlyList<DiscordValidationError> ValidationErrors { get; } = error?.ValidationErrors ?? [];

        /// <summary>
        /// Gets the parsed Discord error payload, when available.
        /// </summary>
        public DiscordApiError? Error => error;

        private static string GetValidMessage(HttpStatusCode statusCode, string? httpReasonPhrase, DiscordApiError? error)
        {
            return string.IsNullOrEmpty(error?.Message)
                ? $"Discord API error ({(int)statusCode} {httpReasonPhrase})."
                : error.Message!;
        }
    }
}
