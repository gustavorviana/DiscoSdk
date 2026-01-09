using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Managers;

/// <summary>
/// Base class for channel manager wrappers.
/// </summary>
/// <typeparam name="TSelf">The type of the implementing class, used for method chaining.</typeparam>
internal abstract class ChannelManagerWrapper<TSelf> : ManagerWrapper<TSelf>, IChannelManager<TSelf> where TSelf : ChannelManagerWrapper<TSelf>, IChannelManager<TSelf>
{
	protected readonly DiscordId _channelId;
	protected readonly ChannelClient _channelClient;
	protected readonly Dictionary<string, object?> _changes = [];

	protected ChannelManagerWrapper(DiscordId channelId, ChannelClient channelClient)
	{
		_channelId = channelId;
		_channelClient = channelClient;
	}

	/// <inheritdoc />
	public TSelf SetName(string name)
	{
		ArgumentNullException.ThrowIfNull(name);
		_changes["name"] = name;
		MarkAsModified("name");
		return (TSelf)this;
	}

	/// <inheritdoc />
	public TSelf SetParent(DiscordId? parentId)
	{
		_changes["parent_id"] = parentId?.ToString();
		MarkAsModified("parent_id");
		return (TSelf)this;
	}

	/// <inheritdoc />
	public TSelf SetPosition(int position)
	{
		_changes["position"] = position;
		MarkAsModified("position");
		return (TSelf)this;
	}

	/// <inheritdoc />
	public TSelf PutPermissionOverride(ulong targetId, bool isRole, DiscordPermission allow, DiscordPermission deny)
	{
		// Permission overrides are complex - this would need to be implemented based on the actual API structure
		// For now, this is a placeholder
		MarkAsModified("permission_overwrites");
		return (TSelf)this;
	}

	/// <inheritdoc />
	public TSelf RemovePermissionOverride(ulong targetId, bool isRole)
	{
		// Permission overrides are complex - this would need to be implemented based on the actual API structure
		// For now, this is a placeholder
		MarkAsModified("permission_overwrites");
		return (TSelf)this;
	}

	/// <inheritdoc />
	public TSelf SyncPermissions()
	{
		_changes["permission_overwrites"] = null;
		MarkAsModified("permission_overwrites");
		return (TSelf)this;
	}

	/// <inheritdoc />
	protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
	{
		if (_changes.Count == 0)
			return;

		await _channelClient.EditAsync(_channelId, _changes, cancellationToken);
	}
}