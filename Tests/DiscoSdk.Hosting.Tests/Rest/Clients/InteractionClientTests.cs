using DiscoSdk;
using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Rest;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class InteractionClientTests
{
    private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
    private readonly InteractionClient _client;
    private readonly InteractionHandle _handle = new(new Snowflake(123ul), "tok");

    public InteractionClientTests()
    {
        var discord = Substitute.For<IDiscordClient>();
        discord.HttpClient.Returns(_http);
        discord.ApplicationId.Returns(new Snowflake(456ul));
        _client = new InteractionClient(discord);
    }

    [Fact]
    public async Task RespondWithAutocompleteAsync_MarksInteractionRespondedAsync()
    {
        Assert.False(_handle.Responded);

        await _client.RespondWithAutocompleteAsync(_handle, new[]
        {
            new SlashCommandOptionChoice { Name = "Apple", Value = "Apple" },
        });

        Assert.True(_handle.Responded);
    }

    [Fact]
    public async Task RespondWithAutocompleteAsync_MarksRespondedEvenIfDiscordRejectsAsync()
    {
        // Discord retorna "Interaction has already been acknowledged" → SendAsync lança.
        // O Responded ainda deve ser setado (no finally) pra evitar que handlers subsequentes
        // do mesmo dispatch tentem responder de novo e gerem outro erro idêntico.
        _http
            .SendAsync(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("simulated discord rejection"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _client.RespondWithAutocompleteAsync(_handle, Array.Empty<SlashCommandOptionChoice>()));

        Assert.True(_handle.Responded);
    }

    [Fact]
    public async Task RespondWithAutocompleteAsync_PostsToCallbackRouteAsync()
    {
        await _client.RespondWithAutocompleteAsync(_handle, Array.Empty<SlashCommandOptionChoice>());

        await _http.Received(1).SendAsync(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"interactions/{_handle.Id}/{_handle.Token}/callback"),
            HttpMethod.Post,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RespondWithAutocompleteAsync_RejectsMoreThan25ChoicesAsync()
    {
        var choices = Enumerable.Range(0, 26)
            .Select(i => new SlashCommandOptionChoice { Name = $"c{i}", Value = $"c{i}" })
            .ToArray();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _client.RespondWithAutocompleteAsync(_handle, choices));

        // No HTTP call should be made when validation fails up front.
        await _http.DidNotReceive().SendAsync(
            Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>());
        Assert.False(_handle.Responded);
    }
}
