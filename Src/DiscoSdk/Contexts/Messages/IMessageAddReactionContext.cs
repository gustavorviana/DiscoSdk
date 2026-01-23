using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Contexts.Messages;

public interface IMessageAddReactionContext : IMemberContext
{
    ITextBasedChannel Channel { get; }
    Snowflake MessageId { get; }
    IUser User { get; }
    IEmoji Emoji { get; }

    IRestAction<IMessage> GetMessage();
}
