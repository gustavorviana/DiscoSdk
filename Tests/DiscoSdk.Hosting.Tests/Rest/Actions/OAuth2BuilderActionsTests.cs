using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.OAuth2;
using DiscoSdk.Rest;
using NSubstitute;
using System.Net.Http.Headers;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

public class OAuth2BuilderActionsTests : WrapperTestBase
{
    public OAuth2BuilderActionsTests()
    {
        // OAuth2 endpoints all need the application id (sent as Basic auth user). The bot client
        // discovers it on READY; in unit tests we set it directly via the internal setter.
        Client.ApplicationId = new Snowflake(555);

        Http.SendAsync<AccessTokenResponse>(
                Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(),
                Arg.Any<AuthenticationHeaderValue?>(), Arg.Any<CancellationToken>())
            .Returns(new AccessTokenResponse());

        Http.SendAsync(
                Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(),
                Arg.Any<AuthenticationHeaderValue?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Exchange_AllFieldsSet_PostsOAuth2TokenAsync()
    {
        await Client.OAuth2.ExchangeAuthorizationCode()
            .SetClientSecret("secret")
            .SetCode("the-code")
            .SetRedirectUri("https://x/y")
            .ExecuteAsync();

        await Http.Received(1).SendAsync<AccessTokenResponse>(
            Arg.Is<DiscordRoute>(r => r.ToString() == "oauth2/token"),
            HttpMethod.Post,
            Arg.Any<object?>(),
            Arg.Is<AuthenticationHeaderValue?>(h => h != null && h.Scheme == "Basic"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Exchange_MissingSecret_ThrowsOnExecuteAsync()
    {
        var action = Client.OAuth2.ExchangeAuthorizationCode().SetCode("c").SetRedirectUri("r");
        await Assert.ThrowsAsync<InvalidOperationException>(() => action.ExecuteAsync());
    }

    [Fact]
    public async Task Exchange_MissingCode_ThrowsOnExecuteAsync()
    {
        var action = Client.OAuth2.ExchangeAuthorizationCode().SetClientSecret("s").SetRedirectUri("r");
        await Assert.ThrowsAsync<InvalidOperationException>(() => action.ExecuteAsync());
    }

    [Fact]
    public async Task Revoke_WithHint_PostsRevokeRouteAsync()
    {
        await Client.OAuth2.RevokeToken()
            .SetClientSecret("secret")
            .SetToken("abc")
            .WithHint(OAuth2TokenTypeHint.AccessToken)
            .ExecuteAsync();

        await Http.Received(1).SendAsync(
            Arg.Is<DiscordRoute>(r => r.ToString() == "oauth2/token/revoke"),
            HttpMethod.Post,
            Arg.Any<object?>(),
            Arg.Is<AuthenticationHeaderValue?>(h => h != null && h.Scheme == "Basic"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Revoke_NoHint_StillExecutesAsync()
    {
        await Client.OAuth2.RevokeToken().SetClientSecret("s").SetToken("t").ExecuteAsync();

        await Http.Received(1).SendAsync(
            Arg.Is<DiscordRoute>(r => r.ToString() == "oauth2/token/revoke"),
            HttpMethod.Post,
            Arg.Any<object?>(),
            Arg.Any<AuthenticationHeaderValue?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Revoke_MissingToken_ThrowsOnExecuteAsync()
    {
        var action = Client.OAuth2.RevokeToken().SetClientSecret("s");
        await Assert.ThrowsAsync<InvalidOperationException>(() => action.ExecuteAsync());
    }
}
