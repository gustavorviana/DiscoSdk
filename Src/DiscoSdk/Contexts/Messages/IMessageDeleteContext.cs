using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Contexts.Messages;

public interface IMessageDeleteContext : IContext
{
    Snowflake Id { get; }
    ITextBasedChannel Channel { get; }
    IGuild? Guild { get; }
}
