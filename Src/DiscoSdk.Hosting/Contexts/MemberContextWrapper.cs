using DiscoSdk.Contexts;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts;

internal class MemberContextWrapper(DiscordClient client, IGuild? guild, IUser author, IMember? member) : ContextWrapper(client), IMemberContext
{
    public IGuild? Guild => guild;

    public IMember? Member => member;

    public IUser Author => author;
}