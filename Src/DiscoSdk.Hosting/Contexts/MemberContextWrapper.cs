using DiscoSdk.Contexts;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts;

internal class MemberContextWrapper(
    DiscordClient client,
    IGuild? guild,
    IUser author,
    Snowflake memberId,
    IMember? member) : ContextWrapper(client), IMemberContext
{
    public IGuild? Guild => guild;

    public IMember? Member => member;

    public Snowflake MemberId => memberId;

    public IUser Author => author;
}