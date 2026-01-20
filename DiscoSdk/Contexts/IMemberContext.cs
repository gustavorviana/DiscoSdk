using DiscoSdk.Models;

namespace DiscoSdk.Contexts;

public interface IMemberContext : IContext
{
    IGuild? Guild { get; }
    IMember? Member { get; }
}