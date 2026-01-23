using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest.Actions;
using System.Collections.Immutable;

namespace DiscoSdk.Hosting.Containers;

/// <summary>
/// Container for channel permissions that implements <see cref="IPermissionContainer"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ChannelPermissionContainer"/> class.
/// </remarks>
/// <param name="channel">The channel to manage permissions for.</param>
/// <param name="client">The Discord client for performing operations.</param>
internal class ChannelPermissionContainer(Channel channel, DiscordClient client) : IPermissionContainer
{
	private readonly Channel _channel = channel ?? throw new ArgumentNullException(nameof(channel));
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

	/// <inheritdoc />
	public PermissionOverwrite[]? PermissionOverwrites => _channel.PermissionOverwrites;

	/// <inheritdoc />
	public PermissionOverride? GetPermissionOverride(IPermissionHolder holder)
	{
		if (_channel.PermissionOverwrites == null)
			return null;

		var holderId = holder.Id;
		var overwrite = _channel.PermissionOverwrites.FirstOrDefault(po => po.Id == holderId);
		return overwrite != null ? new PermissionOverride(overwrite) : null;
	}

	/// <inheritdoc />
	public ImmutableList<PermissionOverride> GetPermissionOverrides()
	{
		if (_channel.PermissionOverwrites == null)
			return [];

		return [.. _channel.PermissionOverwrites.Select(po => new PermissionOverride(po))];
	}

	/// <inheritdoc />
	public OverrideIPermissionAction UpsertPermissionOverride(IPermissionHolder holder)
	{
		return new OverridePermissionAction(_client, _channel.Id, holder.Id);
	}
}