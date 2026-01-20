using DiscoSdk.Contexts.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Hosting.Contexts.Channels;

internal class TypingContextWrapper(DiscordClient client,
    DateTimeOffset startedAt,
    ITextBasedChannel channel,
    IGuild? guild,
    IUser user,
    IMember? member) : MemberContextWrapper(client, guild, user, member), ITypingContext
{
    public DateTimeOffset StartedAt => startedAt;

    public ITextBasedChannel Channel => channel;
}