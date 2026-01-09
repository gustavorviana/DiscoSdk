using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Rest.Actions;
using System;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper that implements <see cref="IInteractionData"/> for a <see cref="InteractionData"/> instance.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InteractionDataWrapper"/> class.
/// </remarks>
/// <param name="data">The interaction data instance to wrap.</param>
internal class InteractionDataWrapper(InteractionData data, ITextBasedChannel channel, DiscordClient client) : IInteractionData
{
    private readonly InteractionData _data = data ?? throw new ArgumentNullException(nameof(data));
    private IInteractionResolved? _resolved = null;

    public IRestAction<IInteractionResolved?> GetResolved()
    {
        return RestAction<IInteractionResolved?>.Create(async cancellationToken =>
        {
            if (data.Resolved is null)
                return null;

            if (_resolved is not null)
                return _resolved;

            var guild = channel is IGuildChannelBase guildChannel ? guildChannel.Guild : null;
            var rolesId = data.Resolved.Roles?.Keys?.ToArray() ?? [];
            var membersId = data.Resolved.Members?.Keys?.ToArray() ?? [];
            var messages = data.Resolved.Messages?.Values?.Select(x => new MessageWrapper(channel, x, client, null))?.ToArray() ?? [];
            IRole[] roles = [];
            IMember[] members = [];
            IUser[] users = [];

            var channels = data.Resolved.Channels?.Values.Select(x => new GuildChannelUnionWrapper(x, guild, client))?.ToArray() ?? [];

            if (guild is not null)
            {
                if (membersId.Length > 0)
                    members = [.. (await guild.GetMembers().ExecuteAsync(cancellationToken)).Where(x => membersId.Contains(x.Id.ToString()))];

                if (rolesId.Length > 0)
                    roles = [.. (await guild.GetRoles().ExecuteAsync(cancellationToken)).Where(x => rolesId.Contains(x.Id.ToString()))];
            }

            return _resolved = new InteractionResolvedWrapper(users, members, roles, channels, messages);
        });
    }

    /// <inheritdoc />
    public DiscordId? Id => _data.Id;

    /// <inheritdoc />
    public string Name => _data.Name;

    /// <inheritdoc />
    public ApplicationCommandType Type => _data.Type;

    /// <inheritdoc />
    public IInteractionResolved? Resolved { get; }

    /// <inheritdoc />
    public InteractionOption[]? Options => _data.Options;

    /// <inheritdoc />
    public string? CustomId => _data.CustomId;

    /// <inheritdoc />
    public ComponentType? ComponentType => _data.ComponentType;

    /// <inheritdoc />
    public string[]? Values => _data.Values;

    /// <inheritdoc />
    public ModalComponent[]? Components => _data.Components;
}