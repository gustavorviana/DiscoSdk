using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;
using System.Reflection;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Wrappers.Common;

/// <summary>
/// Base class for wrapper tests. Builds a real <see cref="DiscordClient"/> and then swaps its
/// <see cref="DiscordClient.HttpClient"/> and every internal REST client backing field for ones
/// wrapping a single NSubstitute-backed <see cref="IDiscordRestClient"/>. The mock never makes
/// network calls — the tests assert what each wrapper sends to it (route / verb / body).
/// </summary>
/// <remarks>
/// Inherited by per-wrapper test classes. xUnit instantiates one test class per test method, so
/// each test gets a fresh <see cref="DiscordClient"/> + mock — no state leaks between tests.
/// </remarks>
public abstract class WrapperTestBase
{
	/// <summary>The mocked REST surface every wrapper call lands on.</summary>
	protected IDiscordRestClient Http { get; }

	/// <summary>The real <see cref="DiscordClient"/>, with all its REST clients re-wired through <see cref="Http"/>.</summary>
	protected DiscordClient Client { get; }

	protected WrapperTestBase()
	{
		Http = Substitute.For<IDiscordRestClient>();
		Http.JsonOptions.Returns(new JsonSerializerOptions());

		// Enable every intent we might exercise from a wrapper test path so intent-guarded
		// actions (reactions, etc.) don't need bespoke setup per test class.
		Client = DiscordClientBuilder.Create("test-token")
			.WithIntents(
				DiscordIntent.Guilds |
				DiscordIntent.GuildMessages |
				DiscordIntent.GuildMessageReactions |
				DiscordIntent.DirectMessages |
				DiscordIntent.DirectMessageReactions |
				DiscordIntent.MessageContent)
			.Build();

		SetBackingField(Client, nameof(DiscordClient.HttpClient), Http);

		var msg = new MessageClient(Http);
		SetBackingField(Client, "MessageClient", msg);
		SetBackingField(Client, "ChannelClient", new ChannelClient(Http, msg));
		SetBackingField(Client, "InviteClient", new InviteClient(Http));
		SetBackingField(Client, "RoleClient", new RoleClient(Http));
		SetBackingField(Client, "GuildClient", new GuildClient(Http));
		SetBackingField(Client, "AutoModerationClient", new AutoModerationClient(Http));
		SetBackingField(Client, "ApplicationClient", new ApplicationClient(Http));
		SetBackingField(Client, "GuildTemplateClient", new GuildTemplateClient(Http));
		SetBackingField(Client, "UserClient", new UserClient(Http));
		SetBackingField(Client, "WebhookClient", new WebhookClient(Http));
	}

	/// <summary>
	/// Returns <c>true</c> if <paramref name="body"/> is a dictionary that contains <paramref name="key"/>
	/// mapped to a value equal to <paramref name="expected"/>. Used to assert body shape on PATCH/POST
	/// requests — every internal action-builder produces an <see cref="IDictionary{TKey, TValue}"/> body.
	/// </summary>
	protected static bool BodyContains(object? body, string key, object? expected)
	{
		if (body is not IDictionary<string, object?> dict) return false;
		return dict.TryGetValue(key, out var v) && Equals(v, expected);
	}

	/// <summary>
	/// Returns <c>true</c> if <paramref name="body"/> is a dictionary that contains <paramref name="key"/>
	/// (regardless of value).
	/// </summary>
	protected static bool BodyHasKey(object? body, string key)
		=> body is IDictionary<string, object?> dict && dict.ContainsKey(key);

	/// <summary>
	/// Returns <c>true</c> if <paramref name="body"/> is an anonymous-style object whose property
	/// <paramref name="propertyName"/> equals <paramref name="expected"/>. Used for builders that pass
	/// `new { foo = bar }` rather than a dictionary.
	/// </summary>
	protected static bool BodyPropertyEquals(object? body, string propertyName, object? expected)
	{
		if (body is null) return false;
		var prop = body.GetType().GetProperty(propertyName);
		if (prop is null) return false;
		return Equals(prop.GetValue(body), expected);
	}

	private static void SetBackingField(object target, string propertyName, object value)
	{
		var field = target.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Missing backing field for property '{propertyName}' on {target.GetType().Name}.");
		field.SetValue(target, value);
	}
}
