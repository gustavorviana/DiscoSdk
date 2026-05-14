using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.OAuth2;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

/// <summary>
/// Unit tests for the OAuth2 authorize-URL builder — pure string composition, no HTTP involved.
/// </summary>
public class BuildAuthorizeUrlActionTests
{
    private const string ClientId = "1234567890";

    private static BuildAuthorizeUrlAction NewAction() => new(ClientId);

    [Fact]
    public void Build_ThrowsWhenNoScopesSet()
    {
        Assert.Throws<InvalidOperationException>(() => NewAction().Build());
    }

    [Fact]
    public void Build_EmitsClientIdResponseTypeCodeAndJoinedScopes()
    {
        var url = NewAction()
            .SetScopes(OAuth2Scope.Identify, OAuth2Scope.Guilds)
            .Build();

        Assert.Contains("https://discord.com/oauth2/authorize?", url);
        Assert.Contains($"client_id={ClientId}", url);
        Assert.Contains("response_type=code", url);
        Assert.Contains("scope=identify%20guilds", url);
    }

    [Fact]
    public void Build_PreservesAllOptionalParams()
    {
        var url = NewAction()
            .SetScopes(OAuth2Scope.Identify)
            .SetResponseType(OAuth2ResponseType.Token)
            .SetRedirectUri("https://my.site/cb")
            .SetState("csrf-token-abc")
            .SetPrompt(OAuth2Prompt.Consent)
            .SetPermissions(8)
            .SetGuildId(new Snowflake(987))
            .SetDisableGuildSelect(true)
            .Build();

        Assert.Contains("response_type=token", url);
        Assert.Contains("redirect_uri=https%3A%2F%2Fmy.site%2Fcb", url);
        Assert.Contains("state=csrf-token-abc", url);
        Assert.Contains("prompt=consent", url);
        Assert.Contains("permissions=8", url);
        Assert.Contains("guild_id=987", url);
        Assert.Contains("disable_guild_select=true", url);
    }

    [Fact]
    public void SetScopes_RejectsEmptyArray()
    {
        Assert.Throws<ArgumentException>(() => NewAction().SetScopes());
    }

    [Fact]
    public void Constructor_RejectsBlankClientId()
    {
        Assert.Throws<ArgumentException>(() => new BuildAuthorizeUrlAction(""));
    }
}
