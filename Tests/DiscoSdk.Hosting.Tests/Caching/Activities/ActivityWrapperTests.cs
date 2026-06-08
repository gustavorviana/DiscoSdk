using DiscoSdk.Hosting.Wrappers.Activities;
using DiscoSdk.Models;
using DiscoSdk.Models.Activities;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Tests.Caching.Activities;

public class ActivityWrapperTests
{
	private static Activity Sample(
		string name = "test",
		ActivityType type = ActivityType.Playing,
		long? createdAt = null,
		int? flags = null,
		bool? instance = null,
		string[]? buttons = null,
		ActivityTimestamps? timestamps = null,
		ActivityEmoji? emoji = null,
		ActivityParty? party = null,
		ActivityAssets? assets = null,
		ActivitySecrets? secrets = null)
		=> new()
		{
			Name = name,
			Type = type,
			CreatedAt = createdAt,
			Flags = flags,
			Instance = instance,
			Buttons = buttons,
			Timestamps = timestamps,
			Emoji = emoji,
			Party = party,
			Assets = assets,
			Secrets = secrets
		};

	[Fact]
	public void CreatedAt_NullPoco_ReturnsNull()
	{
		var wrapper = new ActivityWrapper(Sample(createdAt: null));
		Assert.Null(wrapper.CreatedAt);
	}

	[Fact]
	public void CreatedAt_UnixMs_NormalizedToDateTimeOffset()
	{
		var epoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		var wrapper = new ActivityWrapper(Sample(createdAt: epoch));
		Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(epoch), wrapper.CreatedAt);
	}

	[Fact]
	public void Instance_NullPoco_DefaultsToFalse()
	{
		var wrapper = new ActivityWrapper(Sample(instance: null));
		Assert.False(wrapper.Instance);
	}

	[Fact]
	public void Instance_TruePoco_ReturnsTrue()
	{
		var wrapper = new ActivityWrapper(Sample(instance: true));
		Assert.True(wrapper.Instance);
	}

	[Fact]
	public void Buttons_NullPoco_ReturnsEmpty()
	{
		var wrapper = new ActivityWrapper(Sample(buttons: null));
		Assert.NotNull(wrapper.Buttons);
		Assert.Empty(wrapper.Buttons);
	}

	[Fact]
	public void Buttons_NonNullPoco_PassesThrough()
	{
		var wrapper = new ActivityWrapper(Sample(buttons: ["Join", "Spectate"]));
		Assert.Equal(["Join", "Spectate"], wrapper.Buttons);
	}

	[Fact]
	public void Flags_NullPoco_ReturnsNone()
	{
		var wrapper = new ActivityWrapper(Sample(flags: null));
		Assert.Equal(ActivityFlag.None, wrapper.Flags);
	}

	[Fact]
	public void Flags_RawBitfield_MappedToEnum()
	{
		// Instance | Join | Embedded
		var raw = (int)(ActivityFlag.Instance | ActivityFlag.Join | ActivityFlag.Embedded);
		var wrapper = new ActivityWrapper(Sample(flags: raw));

		Assert.True(wrapper.Flags.HasFlag(ActivityFlag.Instance));
		Assert.True(wrapper.Flags.HasFlag(ActivityFlag.Join));
		Assert.True(wrapper.Flags.HasFlag(ActivityFlag.Embedded));
		Assert.False(wrapper.Flags.HasFlag(ActivityFlag.Spectate));
	}

	[Fact]
	public void Timestamps_NullPoco_ReturnsNull()
	{
		var wrapper = new ActivityWrapper(Sample(timestamps: null));
		Assert.Null(wrapper.Timestamps);
	}

	[Fact]
	public void Timestamps_NonNullPoco_WrappedAndCachedAsync()
	{
		var poco = new ActivityTimestamps { Start = 1, End = 2 };
		var wrapper = new ActivityWrapper(Sample(timestamps: poco));

		var first = wrapper.Timestamps;
		var second = wrapper.Timestamps;

		Assert.NotNull(first);
		Assert.Same(first, second); // lazy-cached wrapper instance
	}

	[Fact]
	public void Emoji_NullPoco_ReturnsNull()
	{
		var wrapper = new ActivityWrapper(Sample(emoji: null));
		Assert.Null(wrapper.Emoji);
	}

	[Fact]
	public void Emoji_NonNullPoco_WrappedAndCached()
	{
		var poco = new ActivityEmoji { Name = "🔥" };
		var wrapper = new ActivityWrapper(Sample(emoji: poco));

		Assert.Same(wrapper.Emoji, wrapper.Emoji);
	}

	[Fact]
	public void Party_NullPoco_ReturnsNull()
	{
		var wrapper = new ActivityWrapper(Sample(party: null));
		Assert.Null(wrapper.Party);
	}

	[Fact]
	public void Party_NonNullPoco_WrappedAndCached()
	{
		var poco = new ActivityParty { Id = "abc", Size = [2, 8] };
		var wrapper = new ActivityWrapper(Sample(party: poco));

		Assert.Same(wrapper.Party, wrapper.Party);
	}

	[Fact]
	public void Assets_NonNullPoco_WrappedAndCached()
	{
		var poco = new ActivityAssets { LargeImage = "key" };
		var wrapper = new ActivityWrapper(Sample(assets: poco));

		Assert.Same(wrapper.Assets, wrapper.Assets);
	}

	[Fact]
	public void Secrets_NonNullPoco_WrappedAndCached()
	{
		var poco = new ActivitySecrets { Join = "token" };
		var wrapper = new ActivityWrapper(Sample(secrets: poco));

		Assert.Same(wrapper.Secrets, wrapper.Secrets);
	}
}
