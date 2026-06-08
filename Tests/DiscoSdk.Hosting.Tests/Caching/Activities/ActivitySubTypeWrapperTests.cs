using DiscoSdk.Hosting.Wrappers.Activities;
using DiscoSdk.Models;
using DiscoSdk.Models.Activities;

namespace DiscoSdk.Hosting.Tests.Caching.Activities;

public class ActivitySubTypeWrapperTests
{
	// ---- ActivityTimestampsWrapper ----

	[Fact]
	public void Timestamps_NullStartEnd_ReturnsNull()
	{
		var wrapper = new ActivityTimestampsWrapper(new ActivityTimestamps());
		Assert.Null(wrapper.Start);
		Assert.Null(wrapper.End);
	}

	[Fact]
	public void Timestamps_UnixMs_NormalizedToDateTimeOffset()
	{
		var startMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		var endMs = startMs + 5_000;

		var wrapper = new ActivityTimestampsWrapper(new ActivityTimestamps { Start = startMs, End = endMs });

		Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(startMs), wrapper.Start);
		Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(endMs), wrapper.End);
	}

	// ---- ActivityEmojiWrapper ----

	[Fact]
	public void Emoji_AnimatedNull_DefaultsToFalse()
	{
		var wrapper = new ActivityEmojiWrapper(new ActivityEmoji { Name = "🔥", Animated = null });
		Assert.False(wrapper.Animated);
	}

	[Fact]
	public void Emoji_AnimatedTrue_PassesThrough()
	{
		var wrapper = new ActivityEmojiWrapper(new ActivityEmoji { Name = "🎉", Animated = true });
		Assert.True(wrapper.Animated);
	}

	[Fact]
	public void Emoji_IdAndName_ExposedDirectly()
	{
		var id = new Snowflake(123);
		var wrapper = new ActivityEmojiWrapper(new ActivityEmoji { Name = "name", Id = id });

		Assert.Equal("name", wrapper.Name);
		Assert.Equal(id, wrapper.Id);
	}

	// ---- ActivityPartyWrapper ----

	[Fact]
	public void Party_NoSize_BothNull()
	{
		var wrapper = new ActivityPartyWrapper(new ActivityParty { Id = "p1", Size = null });
		Assert.Equal("p1", wrapper.Id);
		Assert.Null(wrapper.CurrentSize);
		Assert.Null(wrapper.MaxSize);
	}

	[Fact]
	public void Party_OneElementSize_CurrentOnly()
	{
		var wrapper = new ActivityPartyWrapper(new ActivityParty { Id = "p2", Size = [3] });
		Assert.Equal(3, wrapper.CurrentSize);
		Assert.Null(wrapper.MaxSize);
	}

	[Fact]
	public void Party_TwoElementSize_SplitsTuple()
	{
		var wrapper = new ActivityPartyWrapper(new ActivityParty { Id = "p3", Size = [4, 10] });
		Assert.Equal(4, wrapper.CurrentSize);
		Assert.Equal(10, wrapper.MaxSize);
	}

	// ---- ActivityAssetsWrapper ----

	[Fact]
	public void Assets_AllFields_PassThrough()
	{
		var poco = new ActivityAssets
		{
			LargeImage = "large",
			LargeText = "large-tooltip",
			SmallImage = "small",
			SmallText = "small-tooltip"
		};
		var wrapper = new ActivityAssetsWrapper(poco);

		Assert.Equal("large", wrapper.LargeImage);
		Assert.Equal("large-tooltip", wrapper.LargeText);
		Assert.Equal("small", wrapper.SmallImage);
		Assert.Equal("small-tooltip", wrapper.SmallText);
	}

	// ---- ActivitySecretsWrapper ----

	[Fact]
	public void Secrets_AllFields_PassThrough()
	{
		var poco = new ActivitySecrets
		{
			Join = "join-secret",
			Spectate = "spectate-secret",
			Match = "match-secret"
		};
		var wrapper = new ActivitySecretsWrapper(poco);

		Assert.Equal("join-secret", wrapper.Join);
		Assert.Equal("spectate-secret", wrapper.Spectate);
		Assert.Equal("match-secret", wrapper.Match);
	}
}
