using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

public interface IGuildContext : IContext
{
    IGuild Guild { get; }
}
