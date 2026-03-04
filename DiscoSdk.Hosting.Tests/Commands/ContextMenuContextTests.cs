using DiscoSdk.Hosting.Contexts;
using DiscoSdk.Models;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Users;

namespace DiscoSdk.Hosting.Tests.Commands;

public class ContextMenuContextTests
{
    private static User CreateUser(Snowflake id, string username = "testuser") => new()
    {
        UserId = id,
        Username = username,
        Discriminator = "0"
    };

    private static GuildMember CreateMember(User? user = null) => new()
    {
        User = user,
        Nick = "TestNick",
        Roles = [],
        JoinedAt = "2024-01-01T00:00:00Z"
    };

    private static Message CreateMessage(Snowflake id, string content = "hello") => new()
    {
        Id = id,
        Content = content,
        Author = CreateUser(new Snowflake(999)),
        ChannelId = new Snowflake(100)
    };

    // ── UserCommandContext.ResolveTarget ──

    [Fact]
    public void ResolveTargetUser_WithValidTargetId_ReturnsUser()
    {
        var targetId = new Snowflake(123);
        var user = CreateUser(targetId, "alice");
        var data = new InteractionData
        {
            Name = "Report User",
            TargetId = targetId,
            Resolved = new InteractionResolved
            {
                Users = new Dictionary<string, User> { [targetId.ToString()] = user }
            }
        };

        var (resolvedUser, resolvedMember) = UserCommandContext.ResolveTarget(data, guild: null);

        Assert.NotNull(resolvedUser);
        Assert.Equal("alice", resolvedUser!.Username);
        Assert.Null(resolvedMember);
    }

    [Fact]
    public void ResolveTargetUser_WithMemberInGuild_ReturnsBothUserAndMember()
    {
        var targetId = new Snowflake(456);
        var user = CreateUser(targetId, "bob");
        var member = CreateMember(); // No User field — Discord omits it in resolved members
        var guild = NSubstitute.Substitute.For<IGuild>();

        var data = new InteractionData
        {
            Name = "Kick User",
            TargetId = targetId,
            Resolved = new InteractionResolved
            {
                Users = new Dictionary<string, User> { [targetId.ToString()] = user },
                Members = new Dictionary<string, GuildMember> { [targetId.ToString()] = member }
            }
        };

        var (resolvedUser, resolvedMember) = UserCommandContext.ResolveTarget(data, guild);

        Assert.NotNull(resolvedUser);
        Assert.Equal("bob", resolvedUser!.Username);
        Assert.NotNull(resolvedMember);
        // Verify User field was merged into the member
        Assert.Same(user, resolvedMember!.User);
    }

    [Fact]
    public void ResolveTargetUser_InDm_ReturnsUserWithNullMember()
    {
        var targetId = new Snowflake(789);
        var user = CreateUser(targetId, "carol");
        var data = new InteractionData
        {
            Name = "View Profile",
            TargetId = targetId,
            Resolved = new InteractionResolved
            {
                Users = new Dictionary<string, User> { [targetId.ToString()] = user }
            }
        };

        // guild is null → DM context
        var (resolvedUser, resolvedMember) = UserCommandContext.ResolveTarget(data, guild: null);

        Assert.NotNull(resolvedUser);
        Assert.Equal("carol", resolvedUser!.Username);
        Assert.Null(resolvedMember);
    }

    [Fact]
    public void ResolveTargetUser_NullData_ReturnsNulls()
    {
        var (user, member) = UserCommandContext.ResolveTarget(null, guild: null);

        Assert.Null(user);
        Assert.Null(member);
    }

    [Fact]
    public void ResolveTargetUser_NoTargetId_ReturnsNulls()
    {
        var data = new InteractionData
        {
            Name = "Report User",
            TargetId = null,
            Resolved = new InteractionResolved
            {
                Users = new Dictionary<string, User>
                {
                    ["123"] = CreateUser(new Snowflake(123))
                }
            }
        };

        var (user, member) = UserCommandContext.ResolveTarget(data, guild: null);

        Assert.Null(user);
        Assert.Null(member);
    }

    [Fact]
    public void ResolveTargetUser_MemberAlreadyHasUser_DoesNotOverwrite()
    {
        var targetId = new Snowflake(111);
        var resolvedUser = CreateUser(targetId, "resolved-user");
        var existingUser = CreateUser(targetId, "existing-user");
        var member = CreateMember(existingUser);
        var guild = NSubstitute.Substitute.For<IGuild>();

        var data = new InteractionData
        {
            Name = "Test",
            TargetId = targetId,
            Resolved = new InteractionResolved
            {
                Users = new Dictionary<string, User> { [targetId.ToString()] = resolvedUser },
                Members = new Dictionary<string, GuildMember> { [targetId.ToString()] = member }
            }
        };

        var (_, resolvedMember) = UserCommandContext.ResolveTarget(data, guild);

        // ??= should NOT overwrite the existing user
        Assert.Same(existingUser, resolvedMember!.User);
    }

    // ── MessageCommandContext.ResolveTarget ──

    [Fact]
    public void ResolveTargetMessage_WithValidTargetId_ReturnsMessage()
    {
        var targetId = new Snowflake(555);
        var message = CreateMessage(targetId, "test message");
        var data = new InteractionData
        {
            Name = "Pin Message",
            TargetId = targetId,
            Resolved = new InteractionResolved
            {
                Messages = new Dictionary<string, Message> { [targetId.ToString()] = message }
            }
        };

        var result = MessageCommandContext.ResolveTarget(data);

        Assert.NotNull(result);
        Assert.Equal("test message", result!.Content);
        Assert.Equal(targetId, result.Id);
    }

    [Fact]
    public void ResolveTargetMessage_NullData_ReturnsNull()
    {
        var result = MessageCommandContext.ResolveTarget(null);

        Assert.Null(result);
    }

    [Fact]
    public void ResolveTargetMessage_NoTargetId_ReturnsNull()
    {
        var data = new InteractionData
        {
            Name = "Pin Message",
            TargetId = null,
            Resolved = new InteractionResolved
            {
                Messages = new Dictionary<string, Message>
                {
                    ["555"] = CreateMessage(new Snowflake(555))
                }
            }
        };

        var result = MessageCommandContext.ResolveTarget(data);

        Assert.Null(result);
    }

    [Fact]
    public void ResolveTargetMessage_TargetIdNotInResolved_ReturnsNull()
    {
        var data = new InteractionData
        {
            Name = "Pin Message",
            TargetId = new Snowflake(999),
            Resolved = new InteractionResolved
            {
                Messages = new Dictionary<string, Message>
                {
                    ["555"] = CreateMessage(new Snowflake(555))
                }
            }
        };

        var result = MessageCommandContext.ResolveTarget(data);

        Assert.Null(result);
    }
}
