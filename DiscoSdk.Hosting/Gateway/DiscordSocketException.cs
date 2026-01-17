using System.Net.Sockets;
using System.Net.WebSockets;

namespace DiscoSdk.Hosting.Gateway
{
    public class DiscordSocketException : SocketException
    {
        public DiscordSocketException(WebSocketCloseStatus status, string message) : base((int)status, message)
        {

        }
    }
}
