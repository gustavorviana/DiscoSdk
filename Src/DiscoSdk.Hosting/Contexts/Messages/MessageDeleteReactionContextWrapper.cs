using DiscoSdk.Contexts.Messages;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Contexts.Messages;

internal class MessageDeleteReactionContextWrapper(DiscordClient client,
    ITextBasedChannel channel,
    IGuild? guild,
    Snowflake userId,
    Snowflake messageId,
    IEmoji emoji) : ContextWrapper(client), IMessageDeleteReactionContext
{
    public ITextBasedChannel Channel => channel;

    public IGuild? Guild => guild;

    public Snowflake MessageId => messageId;

    public IEmoji Emoji => emoji;

    public IRestAction<IMessage> GetMessage()
    {
        return RestAction<IMessage>.Create(async cancellationToken =>
        {
            var message = await Client.MessageClient.GetAsync(channel.Id, MessageId, cancellationToken);
            return new MessageWrapper(Client, channel, message, null);
        });
    }

    public IRestAction<IUser> GetUser()
    {
        return Client.Users.Get(userId)!;
    }
}