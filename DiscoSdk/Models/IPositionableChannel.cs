using DiscoSdk.Models.Channels;

namespace DiscoSdk.Models
{
    public interface IPositionableChannel : IGuildChannelBase
    {
        int Position { get; }
    }
}
