using DiscoSdk;
using DiscoSdk.Exceptions;
using NSubstitute;

namespace DiscoSdk.Tests.Exceptions;

public class IntentGuardTests
{
	[Fact]
	public void Require_WhenIntentPresent_DoesNotThrow()
	{
		var configured = DiscordIntent.Guilds | DiscordIntent.GuildMembers;

		IntentGuard.Require(configured, DiscordIntent.GuildMembers, "list guild members");
	}

	[Fact]
	public void Require_WhenIntentMissing_ThrowsMissingIntentException()
	{
		var configured = DiscordIntent.Guilds;

		var ex = Assert.Throws<MissingIntentException>(
			() => IntentGuard.Require(configured, DiscordIntent.GuildMembers, "list guild members"));

		Assert.Equal(DiscordIntent.GuildMembers, ex.RequiredIntent);
		Assert.Contains("list guild members", ex.Message);
		Assert.Contains("GuildMembers", ex.Message);
	}

	[Fact]
	public void Require_WithClientOverload_ChecksClientIntents()
	{
		var client = Substitute.For<IDiscordClient>();
		client.Intents.Returns(DiscordIntent.Guilds);

		var ex = Assert.Throws<MissingIntentException>(
			() => IntentGuard.Require(client, DiscordIntent.MessageContent, "access message content"));

		Assert.Equal(DiscordIntent.MessageContent, ex.RequiredIntent);
	}

	[Fact]
	public void Require_WithNullClient_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(
			() => IntentGuard.Require((IDiscordClient)null!, DiscordIntent.Guilds, "x"));
	}

	[Fact]
	public void Require_OnAggregateFlag_RequiresExactBit()
	{
		var configured = DiscordIntent.AllUnprivileged;

		IntentGuard.Require(configured, DiscordIntent.GuildMessages, "send messages");

		Assert.Throws<MissingIntentException>(
			() => IntentGuard.Require(configured, DiscordIntent.GuildMembers, "fetch members"));
	}
}
