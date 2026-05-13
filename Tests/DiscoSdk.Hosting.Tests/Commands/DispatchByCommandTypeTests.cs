using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Contexts;
using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Gateway.Events;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Users;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Commands;

public class DispatchByCommandTypeTests
{
    private static readonly DiscordClient TestClient = DiscordClientBuilder
        .Create("test-token")
        .WithIntents(DiscordIntent.DirectMessages)
        .Build();

    private static readonly DiscordEventDispatcher Dispatcher = new(TestClient);

    private static InteractionWrapper CreateWrapper(
        InteractionType interactionType,
        InteractionData? data,
        ITextBasedChannel? channel = null)
    {
        var interaction = new Interaction
        {
            Id = new Snowflake(1),
            ApplicationId = new Snowflake(2),
            Type = interactionType,
            Data = data,
            Token = "test-token",
        };

        var handle = new InteractionHandle(interaction.Id, interaction.Token);
        return new InteractionWrapper(interaction, TestClient, handle, channel, member: null);
    }

    // ── GetInteractionContext: ApplicationCommandType routing ──

    [Fact]
    public void GetInteractionContext_ChatInput_ReturnsCommandContext()
    {
        var wrapper = CreateWrapper(
            InteractionType.ApplicationCommand,
            new InteractionData { Name = "test", Type = ApplicationCommandType.ChatInput });

        var context = Dispatcher.GetInteractionContext(wrapper);

        Assert.IsType<CommandContext>(context);
    }

    [Fact]
    public void GetInteractionContext_UserCommand_ReturnsUserCommandContext()
    {
        var targetId = new Snowflake(100);
        var wrapper = CreateWrapper(
            InteractionType.ApplicationCommand,
            new InteractionData
            {
                Name = "Report User",
                Type = ApplicationCommandType.User,
                TargetId = targetId,
                Resolved = new InteractionResolved
                {
                    Users = new Dictionary<string, User>
                    {
                        [targetId.ToString()] = new()
                        {
                            UserId = targetId,
                            Username = "alice",
                            Discriminator = "0"
                        }
                    }
                }
            });

        var context = Dispatcher.GetInteractionContext(wrapper);

        Assert.IsType<UserCommandContext>(context);
    }

    [Fact]
    public void GetInteractionContext_MessageCommand_ReturnsMessageCommandContext()
    {
        var targetId = new Snowflake(200);
        var channel = Substitute.For<ITextBasedChannel>();
        var wrapper = CreateWrapper(
            InteractionType.ApplicationCommand,
            new InteractionData
            {
                Name = "Pin Message",
                Type = ApplicationCommandType.Message,
                TargetId = targetId,
                Resolved = new InteractionResolved
                {
                    Messages = new Dictionary<string, Message>
                    {
                        [targetId.ToString()] = new()
                        {
                            Id = targetId,
                            Content = "hello",
                            Author = new User
                            {
                                UserId = new Snowflake(999),
                                Username = "author",
                                Discriminator = "0"
                            },
                            ChannelId = new Snowflake(300)
                        }
                    }
                }
            },
            channel);

        var context = Dispatcher.GetInteractionContext(wrapper);

        Assert.IsType<MessageCommandContext>(context);
    }

    [Fact]
    public void GetInteractionContext_Autocomplete_ReturnsAutocompleteContext()
    {
        var data = new InteractionData
        {
            Name = "test",
            Type = ApplicationCommandType.ChatInput,
            Options =
            [
                new InteractionOption
                {
                    Name = "query",
                    Type = SlashCommandOptionType.String,
                    Value = "partial",
                    Focused = true
                }
            ],
        };

        var channel = CreateMockChannel();

        var wrapper = CreateWrapper(
            InteractionType.ApplicationCommandAutocomplete,
            data,
            channel);

        var context = Dispatcher.GetInteractionContext(wrapper);

        Assert.IsType<AutocompleteContext>(context);
    }

    [Fact]
    public void GetInteractionContext_ModalSubmit_ReturnsModalContext()
    {
        var wrapper = CreateWrapper(
            InteractionType.ModalSubmit,
            new InteractionData { Name = "test", CustomId = "modal-1" });

        var context = Dispatcher.GetInteractionContext(wrapper);

        Assert.IsType<ModalContext>(context);
    }

    // ── Context type hierarchy verifies dispatch compatibility ──

    [Fact]
    public void UserCommandContext_ImplementsIUserCommandContext()
    {
        // Verifies the dispatcher can cast to IUserCommandContext for IUserCommandHandler dispatch
        var targetId = new Snowflake(100);
        var wrapper = CreateWrapper(
            InteractionType.ApplicationCommand,
            new InteractionData
            {
                Name = "Test",
                Type = ApplicationCommandType.User,
                TargetId = targetId,
                Resolved = new InteractionResolved
                {
                    Users = new Dictionary<string, User>
                    {
                        [targetId.ToString()] = new()
                        {
                            UserId = targetId,
                            Username = "user",
                            Discriminator = "0"
                        }
                    }
                }
            });

        var context = Dispatcher.GetInteractionContext(wrapper);

        Assert.IsAssignableFrom<IUserCommandContext>(context);
        // Also still assignable as IInteractionContext for IInteractionCreateHandler
        Assert.IsAssignableFrom<IInteractionContext>(context);
    }

    [Fact]
    public void MessageCommandContext_ImplementsIMessageCommandContext()
    {
        // Verifies the dispatcher can cast to IMessageCommandContext for IMessageCommandHandler dispatch
        var targetId = new Snowflake(200);
        var channel = Substitute.For<ITextBasedChannel>();
        var wrapper = CreateWrapper(
            InteractionType.ApplicationCommand,
            new InteractionData
            {
                Name = "Test",
                Type = ApplicationCommandType.Message,
                TargetId = targetId,
                Resolved = new InteractionResolved
                {
                    Messages = new Dictionary<string, Message>
                    {
                        [targetId.ToString()] = new()
                        {
                            Id = targetId,
                            Content = "msg",
                            Author = new User
                            {
                                UserId = new Snowflake(999),
                                Username = "a",
                                Discriminator = "0"
                            },
                            ChannelId = new Snowflake(300)
                        }
                    }
                }
            },
            channel);

        var context = Dispatcher.GetInteractionContext(wrapper);

        Assert.IsAssignableFrom<IMessageCommandContext>(context);
        // Also still assignable as IInteractionContext for IInteractionCreateHandler
        Assert.IsAssignableFrom<IInteractionContext>(context);
    }

    [Fact]
    public void ChatInputContext_IsNotAssignableToContextMenuTypes()
    {
        var wrapper = CreateWrapper(
            InteractionType.ApplicationCommand,
            new InteractionData { Name = "test", Type = ApplicationCommandType.ChatInput });

        var context = Dispatcher.GetInteractionContext(wrapper);

        Assert.IsNotType<UserCommandContext>(context);
        Assert.IsNotType<MessageCommandContext>(context);
        Assert.IsType<CommandContext>(context);
    }

    private static ITextBasedChannel CreateMockChannel()
    {
        var channel = Substitute.For<ITextBasedChannel>();
        channel.Id.Returns(new Snowflake(300));
        channel.Name.Returns("test-channel");
        return channel;
    }
}
