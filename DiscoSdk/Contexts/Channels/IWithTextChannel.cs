using DiscoSdk.Models.Channels;

namespace DiscoSdk.Contexts.Channels;

public interface IWithTextChannel : IContext
{
    ITextBasedChannel Channel { get; }
}
