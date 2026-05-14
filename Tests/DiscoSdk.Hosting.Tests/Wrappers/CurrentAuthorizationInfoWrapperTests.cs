using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.OAuth2;
using DiscoSdk.Models.Users;
using AppModel = DiscoSdk.Models.Applications.Application;
using IApp = DiscoSdk.Models.Applications.IApplication;

namespace DiscoSdk.Hosting.Tests.Wrappers;

public class CurrentAuthorizationInfoWrapperTests : WrapperTestBase
{
    [Fact]
    public void ExposesApplicationAndUserViaExistingWrappers()
    {
        var info = new CurrentAuthorizationInfo
        {
            Application = new AppModel { Id = new Snowflake(111), Name = "MyApp", Description = "x" },
            Scopes = new[] { "identify", "guilds" },
            Expires = DateTimeOffset.UnixEpoch.AddDays(1),
            User = new User { UserId = new Snowflake(222), Username = "fulano" },
        };

        var wrapper = new CurrentAuthorizationInfoWrapper(Client, info);

        Assert.IsAssignableFrom<IApp>(wrapper.Application);
        Assert.Equal(new Snowflake(111), wrapper.Application.Id);

        Assert.IsAssignableFrom<IUser>(wrapper.User);
        Assert.Equal(new Snowflake(222), wrapper.User!.Id);

        Assert.Equal(new[] { "identify", "guilds" }, wrapper.Scopes);
        Assert.Equal(DateTimeOffset.UnixEpoch.AddDays(1), wrapper.Expires);
    }

    [Fact]
    public void User_IsNullWhenIdentifyScopeWasNotGranted()
    {
        var info = new CurrentAuthorizationInfo
        {
            Application = new AppModel { Id = new Snowflake(111), Name = "MyApp", Description = "x" },
            Scopes = new[] { "applications.commands.permissions.update" },
            Expires = DateTimeOffset.UtcNow,
            User = null,
        };

        var wrapper = new CurrentAuthorizationInfoWrapper(Client, info);

        Assert.Null(wrapper.User);
    }
}
