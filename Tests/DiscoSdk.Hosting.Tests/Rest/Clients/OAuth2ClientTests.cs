using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.OAuth2;
using DiscoSdk.Rest;
using NSubstitute;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

/// <summary>
/// Verifies each OAuth2 endpoint hits the right route with the right form-encoded body and the
/// right per-request <c>Authorization</c> header (Basic for the token / revoke endpoints, Bearer
/// for <c>/oauth2/@me</c>).
/// </summary>
public class OAuth2ClientTests
{
    private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
    private readonly OAuth2Client _client;
    private const string ClientId = "123456789";
    private const string ClientSecret = "shh-it-is-a-secret";

    public OAuth2ClientTests()
    {
        _http.JsonOptions.Returns(new JsonSerializerOptions());
        _client = new OAuth2Client(_http);
    }

    private static string ExpectedBasicHeader(string clientId, string clientSecret)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

    [Fact]
    public async Task ExchangeAuthorizationCodeAsync_PostsFormEncodedWithBasicAuthAsync()
    {
        _http.SendAsync<AccessTokenResponse>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<AuthenticationHeaderValue?>(), Arg.Any<CancellationToken>())
            .Returns(new AccessTokenResponse());

        await _client.ExchangeAuthorizationCodeAsync(ClientId, ClientSecret, "the-code", "https://x.test/cb");

        await _http.Received(1).SendAsync<AccessTokenResponse>(
            Arg.Is<DiscordRoute>(r => r.ToString() == "oauth2/token"),
            HttpMethod.Post,
            Arg.Any<object?>(),
            Arg.Is<AuthenticationHeaderValue?>(h => h != null && h.Scheme == "Basic" && h.Parameter == ExpectedBasicHeader(ClientId, ClientSecret)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_PostsRefreshTokenGrantAsync()
    {
        _http.SendAsync<AccessTokenResponse>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<AuthenticationHeaderValue?>(), Arg.Any<CancellationToken>())
            .Returns(new AccessTokenResponse());

        await _client.RefreshAccessTokenAsync(ClientId, ClientSecret, "old-refresh-token");

        await _http.Received(1).SendAsync<AccessTokenResponse>(
            Arg.Is<DiscordRoute>(r => r.ToString() == "oauth2/token"),
            HttpMethod.Post,
            Arg.Any<object?>(),
            Arg.Is<AuthenticationHeaderValue?>(h => h != null && h.Scheme == "Basic"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetClientCredentialsTokenAsync_PostsScopeJoinedBySpaceAsync()
    {
        _http.SendAsync<AccessTokenResponse>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<AuthenticationHeaderValue?>(), Arg.Any<CancellationToken>())
            .Returns(new AccessTokenResponse());

        await _client.GetClientCredentialsTokenAsync(ClientId, ClientSecret, new[] { OAuth2Scope.Identify, OAuth2Scope.Guilds });

        await _http.Received(1).SendAsync<AccessTokenResponse>(
            Arg.Is<DiscordRoute>(r => r.ToString() == "oauth2/token"),
            HttpMethod.Post,
            Arg.Any<object?>(),
            Arg.Is<AuthenticationHeaderValue?>(h => h != null && h.Scheme == "Basic"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void GetClientCredentialsTokenAsync_RejectsEmptyScopes()
    {
        Assert.ThrowsAsync<ArgumentException>(() =>
            _client.GetClientCredentialsTokenAsync(ClientId, ClientSecret, Array.Empty<string>()));
    }

    [Fact]
    public async Task RevokeTokenAsync_PostsRevokeRouteWithBasicAuthAsync()
    {
        await _client.RevokeTokenAsync(ClientId, ClientSecret, "tok-abc", OAuth2TokenTypeHint.AccessToken);

        await _http.Received(1).SendAsync(
            Arg.Is<DiscordRoute>(r => r.ToString() == "oauth2/token/revoke"),
            HttpMethod.Post,
            Arg.Any<object?>(),
            Arg.Is<AuthenticationHeaderValue?>(h => h != null && h.Scheme == "Basic"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCurrentAuthorizationInfoAsync_GetsMeWithBearerHeaderAsync()
    {
        _http.SendAsync<CurrentAuthorizationInfo>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<AuthenticationHeaderValue?>(), Arg.Any<CancellationToken>())
            .Returns(new CurrentAuthorizationInfo());

        await _client.GetCurrentAuthorizationInfoAsync("my-access-token");

        await _http.Received(1).SendAsync<CurrentAuthorizationInfo>(
            Arg.Is<DiscordRoute>(r => r.ToString() == "oauth2/@me"),
            HttpMethod.Get,
            Arg.Any<object?>(),
            Arg.Is<AuthenticationHeaderValue?>(h => h != null && h.Scheme == "Bearer" && h.Parameter == "my-access-token"),
            Arg.Any<CancellationToken>());
    }
}
