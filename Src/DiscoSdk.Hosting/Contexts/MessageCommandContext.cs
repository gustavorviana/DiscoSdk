using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Hosting.Wrappers.Messages;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Contexts;

internal class MessageCommandContext : InteractionContextWrapper, IMessageCommandContext
{
    public string Name { get; }
    public IMessage TargetMessage { get; }
    public IRestAction<IInteractionResolved?> Resolved { get; }

    public MessageCommandContext(DiscordClient client, InteractionWrapper interaction)
        : base(client, interaction)
    {
        Name = interaction.Data?.Name ?? string.Empty;
        Resolved = interaction.Data?.GetResolved() ?? RestAction<IInteractionResolved?>.Empty;

        var rawData = interaction.RawInteraction.Data;
        var message = ResolveTarget(rawData);

        TargetMessage = message is not null
            ? new MessageWrapper(client, interaction.Channel, message, null)
            : throw new InvalidOperationException("Message command context requires a target message in resolved data.");
    }

    internal static Message? ResolveTarget(InteractionData? data)
    {
        var targetId = data?.TargetId?.ToString();
        if (targetId is null)
            return null;

        Message? message = null;
        data?.Resolved?.Messages?.TryGetValue(targetId, out message);
        return message;
    }
}
