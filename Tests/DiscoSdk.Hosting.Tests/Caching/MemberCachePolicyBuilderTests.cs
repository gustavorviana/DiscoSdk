using DiscoSdk.Caching;
using DiscoSdk.Hosting.Caching;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using NSubstitute;
using System.Collections.Immutable;

namespace DiscoSdk.Hosting.Tests.Caching;

public class MemberCachePolicyBuilderTests
{
	private static IMember Member(bool isOwner = false, bool isBoosting = false)
	{
		var m = Substitute.For<IMember>();
		m.IsOwner.Returns(isOwner);
		m.IsBoosting.Returns(isBoosting);
		m.IsPending.Returns(false);
		m.VoiceState.Returns((IGuildVoiceState?)null);
		m.OnlineStatus.Returns(OnlineStatus.Offline);
		m.UnsortedRoles.Returns(ImmutableHashSet<IRole>.Empty);
		return m;
	}

	[Fact]
	public void Default_ModeIsAll_RequiringEveryCriterion()
	{
		var policy = new MemberCachePolicyBuilder()
			.IncludeOwner()
			.IncludeBoosters()
			.Build();

		Assert.True(policy.ShouldCache(Member(isOwner: true, isBoosting: true)));
		Assert.False(policy.ShouldCache(Member(isOwner: true, isBoosting: false)));
		Assert.False(policy.ShouldCache(Member(isOwner: false, isBoosting: true)));
	}

	[Fact]
	public void AnyMode_MatchesOnAnyCriterion()
	{
		var policy = new MemberCachePolicyBuilder(PolicyMode.Any)
			.IncludeOwner()
			.IncludeBoosters()
			.Build();

		Assert.True(policy.ShouldCache(Member(isOwner: true, isBoosting: false)));
		Assert.True(policy.ShouldCache(Member(isOwner: false, isBoosting: true)));
		Assert.False(policy.ShouldCache(Member()));
	}

	[Fact]
	public void EmptyAllBuilder_EvaluatesToVacuousTrue()
	{
		var policy = new MemberCachePolicyBuilder().Build();
		Assert.True(policy.ShouldCache(Member()));
	}

	[Fact]
	public void EmptyAnyBuilder_EvaluatesToFalse()
	{
		var policy = new MemberCachePolicyBuilder(PolicyMode.Any).Build();
		Assert.False(policy.ShouldCache(Member(isOwner: true)));
	}

	[Fact]
	public void IncludeAny_ComposesNestedAnyGroup()
	{
		// All-mode root requires every criterion to match; the IncludeAny subgroup itself only
		// needs ONE of its own criteria to satisfy the parent slot.
		var policy = new MemberCachePolicyBuilder(PolicyMode.All)
			.IncludePending()
			.IncludeAny(any => any.IncludeOwner().IncludeBoosters())
			.Build();

		var owner = Substitute.For<IMember>();
		owner.IsOwner.Returns(true);
		owner.IsBoosting.Returns(false);
		owner.IsPending.Returns(true);
		owner.VoiceState.Returns((IGuildVoiceState?)null);
		owner.OnlineStatus.Returns(OnlineStatus.Offline);
		owner.UnsortedRoles.Returns(ImmutableHashSet<IRole>.Empty);
		Assert.True(policy.ShouldCache(owner));

		var booster = Substitute.For<IMember>();
		booster.IsOwner.Returns(false);
		booster.IsBoosting.Returns(true);
		booster.IsPending.Returns(true);
		booster.VoiceState.Returns((IGuildVoiceState?)null);
		booster.OnlineStatus.Returns(OnlineStatus.Offline);
		booster.UnsortedRoles.Returns(ImmutableHashSet<IRole>.Empty);
		Assert.True(policy.ShouldCache(booster));

		var neither = Substitute.For<IMember>();
		neither.IsOwner.Returns(false);
		neither.IsBoosting.Returns(false);
		neither.IsPending.Returns(true);
		neither.VoiceState.Returns((IGuildVoiceState?)null);
		neither.OnlineStatus.Returns(OnlineStatus.Offline);
		neither.UnsortedRoles.Returns(ImmutableHashSet<IRole>.Empty);
		Assert.False(policy.ShouldCache(neither));
	}

	[Fact]
	public void IncludeAll_ComposesNestedAllGroup()
	{
		var policy = new MemberCachePolicyBuilder(PolicyMode.Any)
			.IncludeAll(all => all.IncludeOwner().IncludeBoosters())
			.IncludePending()
			.Build();

		Assert.True(policy.ShouldCache(Member(isOwner: true, isBoosting: true)));
		Assert.False(policy.ShouldCache(Member(isOwner: true, isBoosting: false)));
	}

	[Fact]
	public void IncludeWhere_RoutesThroughPredicate()
	{
		var seen = 0;
		var policy = new MemberCachePolicyBuilder(PolicyMode.Any)
			.IncludeWhere(_ => { seen++; return true; })
			.Build();

		Assert.True(policy.ShouldCache(Member()));
		Assert.Equal(1, seen);
	}

	[Fact]
	public void IncludeBuilder_BuildsAndIncludesNestedPolicy()
	{
		var inner = new MemberCachePolicyBuilder(PolicyMode.Any).IncludeOwner();
		var policy = new MemberCachePolicyBuilder(PolicyMode.All)
			.IncludeBoosters()
			.Include(inner)
			.Build();

		Assert.True(policy.ShouldCache(Member(isOwner: true, isBoosting: true)));
		Assert.False(policy.ShouldCache(Member(isOwner: false, isBoosting: true)));
	}

	[Fact]
	public void Build_TwiceThrows()
	{
		var builder = new MemberCachePolicyBuilder();
		builder.Build();
		Assert.Throws<InvalidOperationException>(() => builder.Build());
	}

	[Fact]
	public void Mutate_AfterBuildThrows()
	{
		var builder = new MemberCachePolicyBuilder();
		builder.Build();
		Assert.Throws<InvalidOperationException>(() => builder.IncludeOwner());
	}
}
