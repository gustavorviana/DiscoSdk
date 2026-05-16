using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Hosting.Models.Requests.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IOverrideIPermissionAction"/> for upserting permission overwrites
/// via <c>PUT /channels/{channel.id}/permissions/{overwrite.id}</c>. Discord returns
/// <c>204 No Content</c>; the action constructs the resulting <see cref="PermissionOverride"/>
/// locally from the values it sent.
/// </summary>
internal class OverridePermissionAction(
	DiscordClient client,
	Snowflake channelId,
	Snowflake holderId,
	PermissionOverwriteType holderType) : RestAction<PermissionOverride>, IOverrideIPermissionAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private DiscordPermission _allow;
	private DiscordPermission _deny;

	/// <inheritdoc />
	public IOverrideIPermissionAction SetAllow(DiscordPermission permissions)
	{
		_allow = permissions;
		return this;
	}

	/// <inheritdoc />
	public IOverrideIPermissionAction SetDeny(DiscordPermission permissions)
	{
		_deny = permissions;
		return this;
	}

	/// <inheritdoc />
	public override async Task<PermissionOverride> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var request = new EditChannelPermissionsRequest
		{
			Allow = _allow,
			Deny = _deny,
			Type = holderType,
		};

		await _client.ChannelClient.EditChannelPermissionsAsync(channelId, holderId, request, cancellationToken);

		return new PermissionOverride(new PermissionOverwrite
		{
			Id = holderId,
			Type = holderType,
			Allow = _allow,
			Deny = _deny,
		});
	}
}
