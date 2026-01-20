using DiscoSdk.Contexts.Messages;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Contexts.Messages
{
    internal class MessageAddReactionContext(DiscordClient client,
    ITextBasedChannel channel,
    IGuild? guild,
    IUser user,
    Snowflake messageId,
    IMember? member,
    IEmoji emoji) : ContextWrapper(client), IMessageAddReactionContext
    {
        public ITextBasedChannel Channel => channel;

        public IGuild? Guild => guild;

        public Snowflake MessageId => messageId;

        public IEmoji Emoji => emoji;

        public IMember? Member => member;

        public IUser User => user;

        public IRestAction<IMessage> GetMessage()
        {
            return RestAction<IMessage>.Create(async cancellationToken =>
            {
                var message = await Client.MessageClient.GetAsync(channel.Id, MessageId, cancellationToken);
                return new MessageWrapper(Client, channel, message, null);
            });
        }
    }
}
