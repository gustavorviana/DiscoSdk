using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Messages;

public interface IMessageCreateContext : IMessageContext
{
    IGuild? Guild { get; }
    IMember? Member { get; }
    IUser Author { get; }
}