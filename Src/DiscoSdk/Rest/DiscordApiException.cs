using System.Net;

namespace DiscoSdk.Rest
{
    /// <summary>
    /// Exception thrown when a Discord API request fails.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DiscordApiException"/> class.
    /// </remarks>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="discordCode">The Discord error code, if available.</param>
    public sealed class DiscordApiException(string message, HttpStatusCode statusCode, int? discordCode) : Exception(message)
    {
        /// <summary>
        /// Gets the HTTP status code returned by the Discord API.
        /// </summary>
        public HttpStatusCode StatusCode => statusCode;

        /// <summary>
        /// Gets the Discord-specific error code, if available.
        /// </summary>
        public int? DiscordCode => discordCode;
    }
}