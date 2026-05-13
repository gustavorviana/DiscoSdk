using DiscoSdk.Contexts.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Hosting.Contexts.Channels;

internal class ChannelPinsUpdateContextWrapper(DiscordClient client,
	ITextBasedChannel channel,
	IGuild? guild,
	DateTimeOffset? lastPinTimestamp) : ContextWrapper(client), IChannelPinsUpdateContext
{
	public ITextBasedChannel Channel => channel;
	public IGuild? Guild => guild;
	public DateTimeOffset? LastPinTimestamp => lastPinTimestamp;
}
