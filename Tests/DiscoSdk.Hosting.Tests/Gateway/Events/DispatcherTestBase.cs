using DiscoSdk.Events;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

/// <summary>
/// Base for <see cref="DiscordEventDispatcher"/> tests. Builds a real <see cref="DiscordClient"/>
/// using the builder (no network — uses <see cref="FakeGatewaySocketFactory"/> for the gateway and
/// a substituted <see cref="IDiscordRestClient"/> for REST). Exposes a fresh dispatcher through
/// <see cref="AddHandler"/> / <see cref="DispatchAsync"/>.
/// </summary>
public abstract class DispatcherTestBase
{
	private readonly DiscordEventDispatcher _dispatcher;

	protected DiscordClient Client { get; }
	protected FakeTimeProvider Time { get; }
	protected IDiscordRestClient Http { get; }

	protected DispatcherTestBase()
	{
		Time = new FakeTimeProvider();
		var socket = new FakeGatewaySocket();
		Http = Substitute.For<IDiscordRestClient>();
		Http.JsonOptions.Returns(new JsonSerializerOptions());

		Client = DiscordClientBuilder.Create("test-token")
			.WithIntents(
				DiscordIntent.Guilds |
				DiscordIntent.GuildMessages |
				DiscordIntent.GuildMessageReactions |
				DiscordIntent.DirectMessages |
				DiscordIntent.DirectMessageReactions |
				DiscordIntent.MessageContent |
				DiscordIntent.GuildMessageTyping)
			.WithLogger(NullLogger.Instance)
			.WithTimeProvider(Time)
			.WithGatewaySocketFactory(new FakeGatewaySocketFactory(socket))
			.WithRestClient(Http)
			.Build();

		_dispatcher = new DiscordEventDispatcher(Client);
	}

	/// <summary>Registers a handler with the dispatcher.</summary>
	protected void AddHandler(IDiscordEventHandler handler) => _dispatcher.Add(handler);

	/// <summary>Runs a single gateway frame through <c>ProcessEventAsync</c>.</summary>
	internal Task DispatchAsync(ReceivedGatewayMessage frame) => _dispatcher.ProcessEventAsync(frame);

	/// <summary>
	/// Configures the mocked REST client so a <c>GET channels/{channelId}</c> returns a
	/// <see cref="ChannelType.GuildText"/> channel tied to <paramref name="guildId"/>. Use before
	/// dispatching frames whose handlers (e.g. interactions) resolve channels via REST.
	/// </summary>
	protected void SeedTextChannel(ulong channelId, ulong? guildId)
	{
		var channel = new Channel
		{
			Id = new Snowflake(channelId),
			Type = ChannelType.GuildText,
			GuildId = guildId is not null ? new Snowflake(guildId.Value) : null,
			Name = "general",
		};

		Http.SendAsync<Channel>(
			Arg.Any<DiscordRoute>(),
			Arg.Any<HttpMethod>(),
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>())
			.Returns(channel);
	}
}
