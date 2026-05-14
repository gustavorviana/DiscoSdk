using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Tests.Wrappers;

public class ApplicationCommandPermissionsWrapperTests
{
    [Fact]
    public void MapsTopLevelFieldsAndWrapsEachPermissionEntry()
    {
        var model = new ApplicationCommandPermissions
        {
            Id = new Snowflake(10),
            ApplicationId = new Snowflake(20),
            GuildId = new Snowflake(30),
            Permissions =
            [
                new ApplicationCommandPermission { Id = new Snowflake(1), Type = ApplicationCommandPermissionType.Role, Permission = true },
                new ApplicationCommandPermission { Id = new Snowflake(2), Type = ApplicationCommandPermissionType.User, Permission = false },
            ],
        };

        var wrapper = new ApplicationCommandPermissionsWrapper(model);

        Assert.Equal(new Snowflake(10), wrapper.Id);
        Assert.Equal(new Snowflake(20), wrapper.ApplicationId);
        Assert.Equal(new Snowflake(30), wrapper.GuildId);
        Assert.Equal(2, wrapper.Permissions.Count);

        Assert.Equal(new Snowflake(1), wrapper.Permissions[0].Id);
        Assert.Equal(ApplicationCommandPermissionType.Role, wrapper.Permissions[0].Type);
        Assert.True(wrapper.Permissions[0].Allowed);

        Assert.Equal(new Snowflake(2), wrapper.Permissions[1].Id);
        Assert.Equal(ApplicationCommandPermissionType.User, wrapper.Permissions[1].Type);
        Assert.False(wrapper.Permissions[1].Allowed);
    }

    [Fact]
    public void EmptyPermissionsListYieldsEmptyWrapperCollection()
    {
        var model = new ApplicationCommandPermissions
        {
            Id = new Snowflake(10),
            ApplicationId = new Snowflake(20),
            GuildId = new Snowflake(30),
            Permissions = [],
        };

        var wrapper = new ApplicationCommandPermissionsWrapper(model);

        Assert.Empty(wrapper.Permissions);
    }
}
