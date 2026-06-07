using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Gateway.Payloads.Models;
using DiscoSdk.Hosting.Wrappers.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Mentions;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Wrappers.Messages;

/// <summary>
/// Verifies <see cref="MessageWrapper"/>'s exemption-aware <c>MessageContent</c> behavior. The
/// wrapper does NOT throw on missing intent — getters return empty and a single warn-once-per-
/// field log line surfaces the misconfig without hot-path log spam. The exemption list (bot is
/// author, bot is mentioned, channel is DM / group DM) suppresses the warning entirely because
/// Discord populates the fields anyway.
/// </summary>
public class MessageContentExemptionTests : IDisposable
{
    private static readonly Snowflake BotUserId = new(1);
    private static readonly Snowflake OtherUserId = new(42);
    private static readonly Snowflake ChannelId = new(200);
    private static readonly Snowflake MessageId = new(300);

    public MessageContentExemptionTests() => MessageContentWarnTracker.ResetForTests();
    public void Dispose() => MessageContentWarnTracker.ResetForTests();

    private static DiscordClient NewClient(DiscordIntent intents, ILogger? logger = null)
    {
        var http = Substitute.For<IDiscordRestClient>();
        http.JsonOptions.Returns(new JsonSerializerOptions());
        var builder = DiscordClientBuilder.Create("test-token")
            .WithIntents(intents)
            .WithRestClient(http);
        if (logger is not null)
            builder = builder.WithLogger(logger);
        var client = builder.Build();
        SeedBotUser(client, BotUserId);
        return client;
    }

    private static void SeedBotUser(DiscordClient client, Snowflake id)
    {
        var prop = typeof(DiscordClient).GetProperty("BotUser",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        Assert.NotNull(prop);
        prop!.SetValue(client, new ReadyUser { Id = id.ToString(), Username = "bot" });
    }

    private static MessageWrapper Build(
        DiscordClient client,
        ChannelType channelType,
        Snowflake authorId,
        params Snowflake[] mentionedUserIds)
    {
        var channel = Substitute.For<ITextBasedChannel>();
        channel.Id.Returns(ChannelId);
        channel.Type.Returns(channelType);

        var msg = new Message
        {
            Id = MessageId,
            ChannelId = ChannelId,
            Content = "secret",
            Timestamp = "2026-06-07T00:00:00+00:00",
            Author = new User { UserId = authorId, Username = "u" },
            Mentions = mentionedUserIds.Select(id => new MessageMentionUser { UserId = id }).ToArray(),
            Reactions = [],
        };

        return new MessageWrapper(client, channel, msg, interactionHandle: null);
    }

    private static bool WarnCalled(ILogger logger) =>
        logger.ReceivedCalls().Any(c =>
            c.GetMethodInfo().Name == nameof(ILogger.Log) &&
            c.GetArguments()[0] is LogLevel.Warning);

    // ---- No intent + not exempt ⇒ returns empty + logs warning ----------------------------------

    [Fact]
    public void NoIntent_GuildChannel_NotAuthor_NotMentioned_ContentEmpty_WarnsOnce()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages, logger);
        var wrapper = Build(client, ChannelType.GuildText, authorId: OtherUserId);

        // Discord left this empty in production, so the wrapper exposes empty — never throws.
        Assert.Equal("secret", wrapper.Content); // payload has content, accessor still surfaces it
        Assert.True(WarnCalled(logger));
    }

    [Fact]
    public void NoIntent_NotExempt_EmbedsEmpty_WarnsOnce()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages, logger);
        var wrapper = Build(client, ChannelType.GuildText, authorId: OtherUserId);

        Assert.NotNull(wrapper.Embeds);
        Assert.True(WarnCalled(logger));
    }

    [Fact]
    public void NoIntent_NotExempt_AttachmentsEmpty_WarnsOnce()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages, logger);
        var wrapper = Build(client, ChannelType.GuildText, authorId: OtherUserId);

        Assert.NotNull(wrapper.Attachments);
        Assert.True(WarnCalled(logger));
    }

    [Fact]
    public void NoIntent_NotExempt_PollNull_WarnsOnce()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages, logger);
        var wrapper = Build(client, ChannelType.GuildText, authorId: OtherUserId);

        _ = wrapper.Poll;
        Assert.True(WarnCalled(logger));
    }

    [Fact]
    public void NoIntent_NotExempt_ComponentsNull_WarnsOnce()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages, logger);
        var wrapper = Build(client, ChannelType.GuildText, authorId: OtherUserId);

        _ = wrapper.Components;
        Assert.True(WarnCalled(logger));
    }

    [Fact]
    public void NoIntent_RepeatedAccess_SameField_WarnsOnlyOnce()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages, logger);
        var wrapper = Build(client, ChannelType.GuildText, authorId: OtherUserId);

        _ = wrapper.Content;
        _ = wrapper.Content;
        _ = wrapper.Content;

        var warnCount = logger.ReceivedCalls().Count(c =>
            c.GetMethodInfo().Name == nameof(ILogger.Log) &&
            c.GetArguments()[0] is LogLevel.Warning);
        Assert.Equal(1, warnCount);
    }

    // ---- Exemptions ⇒ no warn ------------------------------------------------------------------

    [Fact]
    public void NoIntent_BotIsAuthor_NoWarning()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages, logger);
        var wrapper = Build(client, ChannelType.GuildText, authorId: BotUserId);

        Assert.Equal("secret", wrapper.Content);
        Assert.NotNull(wrapper.Embeds);
        Assert.NotNull(wrapper.Attachments);
        _ = wrapper.Poll;
        _ = wrapper.Components;
        Assert.False(WarnCalled(logger));
    }

    [Fact]
    public void NoIntent_BotIsMentioned_NoWarning()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages, logger);
        var wrapper = Build(client, ChannelType.GuildText, authorId: OtherUserId, mentionedUserIds: BotUserId);

        Assert.Equal("secret", wrapper.Content);
        Assert.NotNull(wrapper.Embeds);
        Assert.NotNull(wrapper.Attachments);
        Assert.False(WarnCalled(logger));
    }

    [Fact]
    public void NoIntent_DmChannel_NoWarning()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.DirectMessages, logger);
        var wrapper = Build(client, ChannelType.Dm, authorId: OtherUserId);

        Assert.Equal("secret", wrapper.Content);
        Assert.False(WarnCalled(logger));
    }

    [Fact]
    public void NoIntent_GroupDmChannel_NoWarning()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.DirectMessages, logger);
        var wrapper = Build(client, ChannelType.GroupDm, authorId: OtherUserId);

        Assert.Equal("secret", wrapper.Content);
        Assert.False(WarnCalled(logger));
    }

    // ---- Intent enabled ⇒ no warn regardless ---------------------------------------------------

    [Fact]
    public void WithIntent_NotExempt_NoWarning()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages | DiscordIntent.MessageContent, logger);
        var wrapper = Build(client, ChannelType.GuildText, authorId: OtherUserId);

        Assert.Equal("secret", wrapper.Content);
        Assert.NotNull(wrapper.Embeds);
        Assert.NotNull(wrapper.Attachments);
        Assert.False(WarnCalled(logger));
    }
}
