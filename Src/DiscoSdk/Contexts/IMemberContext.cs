using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Contexts;

public interface IMemberContext : IContext, IGuildContext
{
    IMember? Member { get; }
}