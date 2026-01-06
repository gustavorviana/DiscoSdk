using System.Net;

namespace DiscoSdk.Rest
{
    /// <summary>
    /// Exception thrown when a Discord API request fails.
    /// </summary>
    public sealed class DiscordApiException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code returned by the Discord API.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the Discord-specific error code, if available.
        /// </summary>
        public int? DiscordCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordApiException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="discordCode">The Discord error code, if available.</param>
        public DiscordApiException(string message, HttpStatusCode statusCode, int? discordCode)
            : base(message)
        {
            StatusCode = statusCode;
            DiscordCode = discordCode;
        }
    }

}
