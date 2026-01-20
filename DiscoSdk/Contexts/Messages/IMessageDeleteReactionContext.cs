using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Contexts.Messages;

public interface IMessageDeleteReactionContext : IContext
{
    ITextBasedChannel Channel { get; }

    IGuild? Guild { get; }
    IRestAction<IUser> GetUser();

    Snowflake MessageId {  get; }

    IRestAction<IMessage> GetMessage();

    IEmoji Emoji { get; }
}