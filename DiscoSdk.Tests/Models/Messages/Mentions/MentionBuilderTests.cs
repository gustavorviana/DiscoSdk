using DiscoSdk.Models;
using DiscoSdk.Models.Messages.Mentions;
using DiscoSdk.Rest.Actions.Messages;
using System.Reflection;

namespace DiscoSdk.Tests.Models.Messages.Mentions;

public class MentionBuilderTests
{
    private static HashSet<Mention> GetMentions(MentionBuilder builder)
    {
        var type = typeof(MentionBuilderBase<MentionBuilder>);
        var property = type.GetProperty("Mentions", BindingFlags.NonPublic | BindingFlags.Instance);
        return (HashSet<Mention>)property!.GetValue(builder)!;
    }
    [Fact]
    public void MentionUser_WithDefaultSilent_AddsPingingUserMention()
    {
        // Arrange
        var builder = new MentionBuilder();
        var userId = new Snowflake(123456789012345678UL);

        // Act
        var result = builder.MentionUser(userId);

        // Assert
        Assert.Same(builder, result);
        var mentions = GetMentions(builder);
        Assert.Single(mentions);
        var mention = mentions.First();
        Assert.Equal(MentionType.User, mention.Type);
        Assert.Equal(userId, mention.Id);
        Assert.True(mention.Ping);
    }

    [Fact]
    public void MentionUser_WithSilentTrue_AddsSilentUserMention()
    {
        // Arrange
        var builder = new MentionBuilder();
        var userId = new Snowflake(123456789012345678UL);

        // Act
        var result = builder.MentionUser(userId, ping: false);

        // Assert
        Assert.Same(builder, result);
        var mentions = GetMentions(builder);
        Assert.Single(mentions);
        var mention = mentions.First();
        Assert.Equal(MentionType.User, mention.Type);
        Assert.Equal(userId, mention.Id);
        Assert.False(mention.Ping);
    }

    [Fact]
    public void MentionRole_WithDefaultSilent_AddsPingingRoleMention()
    {
        // Arrange
        var builder = new MentionBuilder();
        var roleId = new Snowflake(987654321098765432UL);

        // Act
        var result = builder.MentionRole(roleId);

        // Assert
        Assert.Same(builder, result);
        var mentions = GetMentions(builder);
        Assert.Single(mentions);
        var mention = mentions.First();
        Assert.Equal(MentionType.Role, mention.Type);
        Assert.Equal(roleId, mention.Id);
        Assert.True(mention.Ping);
    }

    [Fact]
    public void MentionRole_WithSilentTrue_AddsSilentRoleMention()
    {
        // Arrange
        var builder = new MentionBuilder();
        var roleId = new Snowflake(987654321098765432UL);

        // Act
        var result = builder.MentionRole(roleId, ping: false);

        // Assert
        Assert.Same(builder, result);
        var mentions = GetMentions(builder);
        Assert.Single(mentions);
        var mention = mentions.First();
        Assert.Equal(MentionType.Role, mention.Type);
        Assert.Equal(roleId, mention.Id);
        Assert.False(mention.Ping);
    }

    [Fact]
    public void MentionEveryone_WithDefaultSilent_AddsPingingEveryoneMention()
    {
        // Arrange
        var builder = new MentionBuilder();

        // Act
        var result = builder.MentionEveryone();

        // Assert
        Assert.Same(builder, result);
        var mentions = GetMentions(builder);
        Assert.Single(mentions);
        var mention = mentions.First();
        Assert.Equal(MentionType.Everyone, mention.Type);
        Assert.Null(mention.Id);
        Assert.True(mention.Ping);
    }

    [Fact]
    public void MentionEveryone_WithSilentTrue_AddsSilentEveryoneMention()
    {
        // Arrange
        var builder = new MentionBuilder();

        // Act
        var result = builder.MentionEveryone(ping: false);

        // Assert
        Assert.Same(builder, result);
        var mentions = GetMentions(builder);
        Assert.Single(mentions);
        var mention = mentions.First();
        Assert.Equal(MentionType.Everyone, mention.Type);
        Assert.Null(mention.Id);
        Assert.False(mention.Ping);
    }

    [Fact]
    public void AppendMention_WithUserMention_TracksMention()
    {
        // Arrange
        var builder = new MentionBuilder();
        var mention = new Mention(MentionType.User, new Snowflake(123456789012345678UL), ping: true);

        // Act
        var result = builder.AppendMention(mention);

        // Assert
        Assert.Same(builder, result);
        var mentions = GetMentions(builder);
        Assert.Single(mentions);
        Assert.Contains(mention, mentions);
    }

    [Fact]
    public void AppendMention_WithChannelMention_DoesNotTrackMention()
    {
        // Arrange
        var builder = new MentionBuilder();
        var mention = Mention.FromChannel(new Snowflake(111111111111111111UL));

        // Act
        var result = builder.AppendMention(mention);

        // Assert
        Assert.Same(builder, result);
        var mentions = GetMentions(builder);
        Assert.Empty(mentions);
    }

    [Fact]
    public void AppendMention_WithMultipleMentions_TracksAllNonChannelMentions()
    {
        // Arrange
        var builder = new MentionBuilder();
        var userMention = new Mention(MentionType.User, new Snowflake(123456789012345678UL), ping: true);
        var roleMention = new Mention(MentionType.Role, new Snowflake(987654321098765432UL), ping: false);
        var channelMention = Mention.FromChannel(new Snowflake(111111111111111111UL));
        var everyoneMention = Mention.Everyone(ping: true);

        // Act
        builder.AppendMention(userMention)
            .AppendMention(roleMention)
            .AppendMention(channelMention)
            .AppendMention(everyoneMention);

        // Assert
        var mentions = GetMentions(builder);
        Assert.Equal(3, mentions.Count);
        Assert.Contains(userMention, mentions);
        Assert.Contains(roleMention, mentions);
        Assert.Contains(everyoneMention, mentions);
        Assert.DoesNotContain(channelMention, mentions);
    }

    [Fact]
    public void BuildAllowedMentions_WithNoMentions_ReturnsNull()
    {
        // Arrange
        var builder = new MentionBuilder();

        // Act
        var result = builder.BuildAllowedMentions();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void BuildAllowedMentions_WithSinglePingingUser_ReturnsParseUsers()
    {
        // Arrange
        var builder = new MentionBuilder();
        var userId = new Snowflake(123456789012345678UL);
        builder.MentionUser(userId);

        // Act
        var result = builder.BuildAllowedMentions();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("users", result.Parse ?? Array.Empty<string>());
        Assert.Empty(result.Users!);
        Assert.Empty(result.Roles!);
    }

    [Fact]
    public void BuildAllowedMentions_WithSinglePingingRole_ReturnsParseRoles()
    {
        // Arrange
        var builder = new MentionBuilder();
        var roleId = new Snowflake(987654321098765432UL);
        builder.MentionRole(roleId);

        // Act
        var result = builder.BuildAllowedMentions();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("roles", result.Parse ?? Array.Empty<string>());
        Assert.Empty(result.Users!);
        Assert.Empty(result.Roles!);
    }

    [Fact]
    public void BuildAllowedMentions_WithPingingEveryone_ReturnsParseEveryone()
    {
        // Arrange
        var builder = new MentionBuilder();
        builder.MentionEveryone();

        // Act
        var result = builder.BuildAllowedMentions();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("everyone", result.Parse ?? Array.Empty<string>());
        Assert.Empty(result.Users!);
        Assert.Empty(result.Roles!);
    }

    [Fact]
    public void BuildAllowedMentions_WithSilentEveryone_DoesNotReturnParseEveryone()
    {
        // Arrange
        var builder = new MentionBuilder();
        builder.MentionEveryone(ping: false);

        // Act
        var result = builder.BuildAllowedMentions();

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain("everyone", result.Parse ?? Array.Empty<string>());
    }

    [Fact]
    public void BuildAllowedMentions_WithMixedPingingAndSilentUsers_ReturnsExplicitUserIds()
    {
        // Arrange
        var builder = new MentionBuilder();
        var pingingUserId = new Snowflake(123456789012345678UL);
        var silentUserId = new Snowflake(111111111111111111UL);
        builder.MentionUser(pingingUserId)
            .MentionUser(silentUserId, ping: false);

        // Act
        var result = builder.BuildAllowedMentions();

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain("users", result.Parse ?? Array.Empty<string>());
        Assert.NotNull(result.Users);
        Assert.Single(result.Users);
        Assert.Contains(pingingUserId.ToString(), result.Users);
        Assert.DoesNotContain(silentUserId.ToString(), result.Users);
    }

    [Fact]
    public void BuildAllowedMentions_WithMixedPingingAndSilentRoles_ReturnsExplicitRoleIds()
    {
        // Arrange
        var builder = new MentionBuilder();
        var pingingRoleId = new Snowflake(987654321098765432UL);
        var silentRoleId = new Snowflake(222222222222222222UL);
        builder.MentionRole(pingingRoleId)
            .MentionRole(silentRoleId, ping: false);

        // Act
        var result = builder.BuildAllowedMentions();

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain("roles", result.Parse ?? Array.Empty<string>());
        Assert.NotNull(result.Roles);
        Assert.Single(result.Roles);
        Assert.Contains(pingingRoleId.ToString(), result.Roles);
        Assert.DoesNotContain(silentRoleId.ToString(), result.Roles);
    }

    [Fact]
    public void BuildAllowedMentions_WithAllPingingMentions_ReturnsAllParseTypes()
    {
        // Arrange
        var builder = new MentionBuilder();
        builder.MentionUser(new Snowflake(123456789012345678UL))
            .MentionRole(new Snowflake(987654321098765432UL))
            .MentionEveryone();

        // Act
        var result = builder.BuildAllowedMentions();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Parse);
        Assert.Contains("users", result.Parse);
        Assert.Contains("roles", result.Parse);
        Assert.Contains("everyone", result.Parse);
        Assert.Empty(result.Users!);
        Assert.Empty(result.Roles!);
    }

    [Fact]
    public void BuildAllowedMentions_WithMultiplePingingUsers_ReturnsParseUsers()
    {
        // Arrange
        var builder = new MentionBuilder();
        builder.MentionUser(new Snowflake(123456789012345678UL))
            .MentionUser(new Snowflake(111111111111111111UL))
            .MentionUser(new Snowflake(222222222222222222UL));

        // Act
        var result = builder.BuildAllowedMentions();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("users", result.Parse ?? Array.Empty<string>());
        Assert.Empty(result.Users!);
    }

    [Fact]
    public void BuildAllowedMentions_WithOnlySilentMentions_ReturnsEmptyAllowedMentions()
    {
        // Arrange
        var builder = new MentionBuilder();
        builder.MentionUser(new Snowflake(123456789012345678UL), ping: false)
            .MentionRole(new Snowflake(987654321098765432UL), ping: false)
            .MentionEveryone(ping: false);

        // Act
        var result = builder.BuildAllowedMentions();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Parse ?? Array.Empty<string>());
        Assert.Empty(result.Users!);
        Assert.Empty(result.Roles!);
    }

    [Fact]
    public void BuildAllowedMentions_WithChannelMention_IgnoresChannel()
    {
        // Arrange
        var builder = new MentionBuilder();
        var channelId = new Snowflake(111111111111111111UL);
        builder.AppendMention(Mention.FromChannel(channelId))
            .MentionUser(new Snowflake(123456789012345678UL));

        // Act
        var result = builder.BuildAllowedMentions();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("users", result.Parse ?? Array.Empty<string>());
        Assert.Empty(result.Users!);
        Assert.Empty(result.Roles!);
    }

    [Fact]
    public void FluentInterface_AllowsMethodChaining()
    {
        // Arrange
        var builder = new MentionBuilder();
        var userId1 = new Snowflake(123456789012345678UL);
        var userId2 = new Snowflake(111111111111111111UL);
        var roleId = new Snowflake(987654321098765432UL);

        // Act
        var result = builder
            .MentionUser(userId1)
            .MentionUser(userId2, ping: false)
            .MentionRole(roleId)
            .MentionEveryone();

        // Assert
        Assert.Same(builder, result);
        var mentions = GetMentions(builder);
        Assert.Equal(4, mentions.Count);
    }
}

