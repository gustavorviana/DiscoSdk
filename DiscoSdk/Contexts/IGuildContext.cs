using DiscoSdk.Models;

namespace DiscoSdk.Contexts;

public interface IGuildContext : IContext
{
    IGuild Guild { get; }
}
