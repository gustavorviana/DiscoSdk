using DiscoSdk.Models;
using DiscoSdk.Models.Messages;

namespace DiscoSdk.Rest.Actions.Messages;

public interface IBotMessageBuilder<TSelf> : IMessageBuilderAction<TSelf, IMessage>
{
    TSelf SetStickers(IEnumerable<Snowflake> stickers);
}
