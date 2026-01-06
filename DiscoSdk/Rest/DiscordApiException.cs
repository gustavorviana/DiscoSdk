using System.Net;

namespace DiscoSdk.Rest
{
    public sealed class DiscordApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public int? DiscordCode { get; }

        public DiscordApiException(string message, HttpStatusCode statusCode, int? discordCode)
            : base(message)
        {
            StatusCode = statusCode;
            DiscordCode = discordCode;
        }
    }

}
