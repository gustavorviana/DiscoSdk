using System.Net.Sockets;
using System.Net.WebSockets;

namespace DiscoSdk.Hosting.Gateway
{
    public class DiscordSocketException : SocketException
    {
        public DiscordSocketException(WebSocketCloseStatus status, string message) : base((int)status, message)
        {
        }

        /// <summary>
        /// Raw WebSocket close code as received from the gateway. Discord-specific codes
        /// (4000–4014) are not named members of <see cref="WebSocketCloseStatus"/> but are
        /// preserved here via the underlying integer value.
        /// </summary>
        public int CloseCode => ErrorCode;
    }
}
