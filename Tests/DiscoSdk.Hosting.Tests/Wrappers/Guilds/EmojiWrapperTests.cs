using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Guilds;

public class EmojiWrapperTests : WrapperTestBase
{
	private readonly IGuild _guild;

	public EmojiWrapperTests()
	{
		_guild = Substitute.For<IGuild>();
		_guild.Id.Returns(new Snowflake(100));
	}

	private static Emoji Model() => new()
	{
		Id = new Snowflake(42),
		Name = "smile",
		Roles = [],
		User = new User { UserId = new Snowflake(7), Username = "u" },
	};

	[Fact]
	public async Task Edit_PatchesEmojiAsync()
	{
		Http.SendAsync<Emoji>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(Model());
		var wrapper = new EmojiWrapper(Client, Model(), _guild);

		await wrapper.Edit().SetName("new").ExecuteAsync();

		await Http.Received(1).SendAsync<Emoji>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/emojis/42"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyContains(b, "name", "new")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Delete_DeletesEmojiAsync()
	{
		var wrapper = new EmojiWrapper(Client, Model(), _guild);

		await wrapper.Delete().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/emojis/42"),
			HttpMethod.Delete, Arg.Any<CancellationToken>());
	}
}
