using DiscoSdk;
using DiscoSdk.Hosting.Containers;
using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Requests.Channels;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Containers;

/// <summary>
/// Verifies the container's role/member type detection feeds the action correctly and that the
/// delete path hits <c>DELETE /channels/{id}/permissions/{overwrite.id}</c>.
/// </summary>
public class ChannelPermissionContainerTests : WrapperTestBase
{
	private readonly Snowflake _channelId = new(900);
	private readonly Snowflake _holderId = new(901);

	private ChannelPermissionContainer NewContainer()
		=> new(new Channel { Id = _channelId }, Client);

	[Fact]
	public async Task UpsertPermissionOverride_ForRole_SendsRoleTypeAsync()
	{
		var role = Substitute.For<IRole>();
		role.Id.Returns(_holderId);

		await NewContainer().UpsertPermissionOverride(role).ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/permissions/{_holderId}"),
			HttpMethod.Put,
			Arg.Is<object?>(b => (b as EditChannelPermissionsRequest)!.Type == PermissionOverwriteType.Role),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UpsertPermissionOverride_ForMember_SendsMemberTypeAsync()
	{
		var member = Substitute.For<IMember>();
		member.Id.Returns(_holderId);

		await NewContainer().UpsertPermissionOverride(member).ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/permissions/{_holderId}"),
			HttpMethod.Put,
			Arg.Is<object?>(b => (b as EditChannelPermissionsRequest)!.Type == PermissionOverwriteType.Member),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeletePermissionOverride_DeletesPermissionsRouteAsync()
	{
		var role = Substitute.For<IRole>();
		role.Id.Returns(_holderId);

		await NewContainer().DeletePermissionOverride(role).ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/permissions/{_holderId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}
}
