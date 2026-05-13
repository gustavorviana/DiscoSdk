using DiscoSdk.Hosting.Builders;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Wrappers.Common;

/// <summary>
/// Base class for wrapper tests. Builds a real <see cref="DiscordClient"/> with an NSubstitute-backed
/// <see cref="IDiscordRestClient"/> injected through <see cref="DiscordClientBuilder.WithRestClient"/>.
/// The mock never makes network calls — tests assert what each wrapper sends to it (route / verb / body).
/// </summary>
/// <remarks>
/// xUnit instantiates one test class per test method, so each test gets a fresh
/// <see cref="DiscordClient"/> + mock — no state leaks between tests.
/// </remarks>
public abstract class WrapperTestBase
{
	/// <summary>The mocked REST surface every wrapper call lands on.</summary>
	protected IDiscordRestClient Http { get; }

	/// <summary>The real <see cref="DiscordClient"/>, wired to <see cref="Http"/> for all REST calls.</summary>
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
			.WithRestClient(Http)
			.Build();
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
}
