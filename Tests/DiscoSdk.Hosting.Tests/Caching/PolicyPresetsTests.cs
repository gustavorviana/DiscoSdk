using DiscoSdk.Caching;
using DiscoSdk.Hosting.Caching;
using DiscoSdk.Hosting.Caching.Policies;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using NSubstitute;
using System.Collections.Immutable;

namespace DiscoSdk.Hosting.Tests.Caching;

public class PolicyPresetsTests
{
	private static IMember Member(
		bool isOwner = false,
		bool isBoosting = false,
		bool isPending = false,
		IGuildVoiceState? voiceState = null,
		OnlineStatus status = OnlineStatus.Offline,
		IEnumerable<Snowflake>? roleIds = null)
	{
		var member = Substitute.For<IMember>();
		member.IsOwner.Returns(isOwner);
		member.IsBoosting.Returns(isBoosting);
		member.IsPending.Returns(isPending);
		member.VoiceState.Returns(voiceState);
		member.OnlineStatus.Returns(status);

		var roles = ImmutableHashSet<IRole>.Empty;
		if (roleIds is not null)
		{
			var builder = ImmutableHashSet.CreateBuilder<IRole>();
			foreach (var id in roleIds)
			{
				var role = Substitute.For<IRole>();
				role.Id.Returns(id);
				builder.Add(role);
			}
			roles = builder.ToImmutable();
		}
		member.UnsortedRoles.Returns(roles);
		return member;
	}

	[Fact]
	public void AllPolicy_AlwaysReturnsTrue()
	{
		Assert.True(AllPolicy.Instance.ShouldCache(Member()));
	}

	[Fact]
	public void NonePolicy_AlwaysReturnsFalse()
	{
		Assert.False(NonePolicy.Instance.ShouldCache(Member(isOwner: true, isBoosting: true)));
	}

	[Fact]
	public void OwnerPolicy_MatchesOnlyOwner()
	{
		Assert.True(OwnerPolicy.Instance.ShouldCache(Member(isOwner: true)));
		Assert.False(OwnerPolicy.Instance.ShouldCache(Member(isOwner: false)));
	}

	[Fact]
	public void BoosterPolicy_MatchesOnlyBoosters()
	{
		Assert.True(BoosterPolicy.Instance.ShouldCache(Member(isBoosting: true)));
		Assert.False(BoosterPolicy.Instance.ShouldCache(Member(isBoosting: false)));
	}

	[Fact]
	public void PendingPolicy_MatchesOnlyPending()
	{
		Assert.True(PendingPolicy.Instance.ShouldCache(Member(isPending: true)));
		Assert.False(PendingPolicy.Instance.ShouldCache(Member(isPending: false)));
	}

	[Fact]
	public void VoicePolicy_RequiresNonNullVoiceState()
	{
		var voice = Substitute.For<IGuildVoiceState>();
		Assert.True(VoicePolicy.Instance.ShouldCache(Member(voiceState: voice)));
		Assert.False(VoicePolicy.Instance.ShouldCache(Member(voiceState: null)));
	}

	[Theory]
	[InlineData(OnlineStatus.Online, false)]
	[InlineData(OnlineStatus.Idle, false)]
	[InlineData(OnlineStatus.DoNotDisturb, false)]
	[InlineData(OnlineStatus.Offline, false)]
	[InlineData(OnlineStatus.Invisible, true)]
	public void OnlinePolicy_MatchesAccordingToConfiguredStatusFilter(OnlineStatus status, bool expected)
	{
		Assert.Equal(expected, OnlinePolicy.Instance.ShouldCache(Member(status: status)));
	}

	[Fact]
	public void RolesPolicy_MatchesWhenAnyRoleIdIsHeld()
	{
		var wanted = new Snowflake(1);
		var policy = new RolesPolicy([wanted]);

		Assert.True(policy.ShouldCache(Member(roleIds: [wanted])));
		Assert.True(policy.ShouldCache(Member(roleIds: [new Snowflake(2), wanted])));
		Assert.False(policy.ShouldCache(Member(roleIds: [new Snowflake(3)])));
		Assert.False(policy.ShouldCache(Member()));
	}

	[Fact]
	public void RolesPolicy_EmptyConfigurationRejectsEveryone()
	{
		var policy = new RolesPolicy([]);
		Assert.False(policy.ShouldCache(Member(roleIds: [new Snowflake(1)])));
	}

	[Fact]
	public void PredicatePolicy_DelegatesToSuppliedFunction()
	{
		var policy = new PredicatePolicy(m => m.IsOwner);
		Assert.True(policy.ShouldCache(Member(isOwner: true)));
		Assert.False(policy.ShouldCache(Member(isOwner: false)));
	}

	[Fact]
	public void ToPolicy_NoneResolvesToNonePolicy()
	{
		Assert.Same(NonePolicy.Instance, MemberCachePolicy.None.ToPolicy());
	}

	[Fact]
	public void ToPolicy_AllResolvesToAllPolicy()
	{
		Assert.Same(AllPolicy.Instance, MemberCachePolicy.All.ToPolicy());
	}

	[Fact]
	public void ToPolicy_OwnerResolvesToOwnerPolicy()
	{
		Assert.Same(OwnerPolicy.Instance, MemberCachePolicy.Owner.ToPolicy());
	}

	[Fact]
	public void ToPolicy_VoiceResolvesToVoicePolicy()
	{
		Assert.Same(VoicePolicy.Instance, MemberCachePolicy.Voice.ToPolicy());
	}

	[Fact]
	public void ToPolicy_OnlineResolvesToOnlinePolicy()
	{
		Assert.Same(OnlinePolicy.Instance, MemberCachePolicy.Online.ToPolicy());
	}

	[Fact]
	public void ToPolicy_UnknownPresetThrows()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => ((MemberCachePolicy)999).ToPolicy());
	}
}
