using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Requests.Channels;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

/// <summary>
/// Tests for <see cref="OverridePermissionAction"/> — verifies the PUT route, body shape, and the
/// locally-constructed <see cref="PermissionOverride"/> returned to the caller (Discord returns 204).
/// </summary>
public class OverridePermissionActionTests : WrapperTestBase
{
	private readonly Snowflake _channelId = new(111);
	private readonly Snowflake _holderId = new(222);

	[Fact]
	public async Task ExecuteAsync_PutsAllowDenyAndTypeForRoleAsync()
	{
		var action = new OverridePermissionAction(Client, _channelId, _holderId, PermissionOverwriteType.Role)
			.SetAllow(DiscordPermission.SendMessages)
			.SetDeny(DiscordPermission.MentionEveryone);

		var result = await action.ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/permissions/{_holderId}"),
			HttpMethod.Put,
			Arg.Is<object?>(b =>
				(b as EditChannelPermissionsRequest)!.Allow == DiscordPermission.SendMessages &&
				(b as EditChannelPermissionsRequest)!.Deny == DiscordPermission.MentionEveryone &&
				(b as EditChannelPermissionsRequest)!.Type == PermissionOverwriteType.Role),
			Arg.Any<CancellationToken>());

		Assert.Equal(_holderId, result.Id);
		Assert.Equal(PermissionOverwriteType.Role, result.Type);
		Assert.Equal(DiscordPermission.SendMessages, result.Overwrite.Allow);
		Assert.Equal(DiscordPermission.MentionEveryone, result.Overwrite.Deny);
	}

	[Fact]
	public async Task ExecuteAsync_DefaultsAllowAndDenyToZeroWhenNotSetAsync()
	{
		var action = new OverridePermissionAction(Client, _channelId, _holderId, PermissionOverwriteType.Member);

		var result = await action.ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/permissions/{_holderId}"),
			HttpMethod.Put,
			Arg.Is<object?>(b =>
				(b as EditChannelPermissionsRequest)!.Allow == default &&
				(b as EditChannelPermissionsRequest)!.Deny == default &&
				(b as EditChannelPermissionsRequest)!.Type == PermissionOverwriteType.Member),
			Arg.Any<CancellationToken>());

		Assert.Equal(PermissionOverwriteType.Member, result.Type);
	}
}
