using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Requests.Channels;
using DiscoSdk.Rest;
using NSubstitute;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

/// <summary>
/// Behavioural tests for <see cref="CreateGroupDmAction"/>. Uses a real <see cref="DiscordClient"/>
/// with a mocked <see cref="IDiscordRestClient"/> so the action runs end-to-end against an
/// asserted HTTP request body.
/// </summary>
public class CreateGroupDmActionTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly DiscordClient _client;

	public CreateGroupDmActionTests()
	{
		_http.JsonOptions.Returns(new JsonSerializerOptions());

		_client = DiscordClientBuilder.Create("test-token")
			.WithIntents(DiscordIntent.None)
			.WithRestClient(_http)
			.Build();
	}

	private static Channel GroupDmChannel() => new()
	{
		Id = new Snowflake(99),
		Type = ChannelType.GroupDm,
	};

	[Fact]
	public async Task ExecuteAsync_PostsAccessTokensToMeChannelsAsync()
	{
		CreateGroupDmRequest? capturedRequest = null;
		_http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(call =>
			{
				capturedRequest = call.Arg<object?>() as CreateGroupDmRequest;
				return GroupDmChannel();
			});

		await _client.CreateGroupDm()
			.AddRecipient("token-a")
			.AddRecipient("token-b")
			.ExecuteAsync();

		await _http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "users/@me/channels"),
			HttpMethod.Post,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());

		Assert.NotNull(capturedRequest);
		Assert.Equal(new[] { "token-a", "token-b" }, capturedRequest!.AccessTokens);
		Assert.Null(capturedRequest.Nicks);
	}

	[Fact]
	public async Task ExecuteAsync_WithNicks_BuildsCorrectMapAsync()
	{
		CreateGroupDmRequest? capturedRequest = null;
		_http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(call =>
			{
				capturedRequest = call.Arg<object?>() as CreateGroupDmRequest;
				return GroupDmChannel();
			});

		await _client.CreateGroupDm()
			.AddRecipient("token-a", new Snowflake(100), "Alice")
			.AddRecipient("token-b")
			.AddRecipient("token-c", new Snowflake(300), "Charlie")
			.ExecuteAsync();

		Assert.NotNull(capturedRequest);
		Assert.Equal(new[] { "token-a", "token-b", "token-c" }, capturedRequest!.AccessTokens);
		Assert.NotNull(capturedRequest.Nicks);
		Assert.Equal(2, capturedRequest.Nicks!.Count);
		Assert.Equal("Alice", capturedRequest.Nicks["100"]);
		Assert.Equal("Charlie", capturedRequest.Nicks["300"]);
	}

	[Fact]
	public async Task ExecuteAsync_ReturnsGroupDmChannelWrapperAsync()
	{
		_http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(GroupDmChannel());

		var result = await _client.CreateGroupDm()
			.AddRecipient("token-a")
			.AddRecipient("token-b")
			.ExecuteAsync();

		Assert.NotNull(result);
		Assert.Equal(new Snowflake(99), result.Id);
	}

	[Fact]
	public async Task ExecuteAsync_FewerThan2Recipients_ThrowsAsync()
	{
		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			_client.CreateGroupDm().AddRecipient("only-one").ExecuteAsync());

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			_client.CreateGroupDm().ExecuteAsync());

		// No HTTP should have been dispatched.
		await _http.DidNotReceive().SendAsync<Channel>(
			Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public void AddRecipient_AboveLimit_Throws()
	{
		var action = _client.CreateGroupDm();
		for (var i = 0; i < CreateGroupDmAction.MaxRecipients; i++)
			action.AddRecipient($"token-{i}");

		Assert.Throws<InvalidOperationException>(() => action.AddRecipient("token-overflow"));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void AddRecipient_BlankToken_Throws(string? token)
	{
		var action = _client.CreateGroupDm();

		// ArgumentException.ThrowIfNullOrWhiteSpace throws ArgumentNullException for null and
		// ArgumentException for empty/whitespace — both derive from ArgumentException.
		Assert.ThrowsAny<ArgumentException>(() => action.AddRecipient(token!));
	}

	[Fact]
	public void AddRecipientWithNick_BlankNick_Throws()
	{
		var action = _client.CreateGroupDm();

		Assert.Throws<ArgumentException>(() => action.AddRecipient("token-a", new Snowflake(1), "   "));
	}

	[Fact]
	public void AddRecipientWithNick_DefaultUserId_Throws()
	{
		var action = _client.CreateGroupDm();

		Assert.Throws<ArgumentException>(() => action.AddRecipient("token-a", default, "Alice"));
	}

}
