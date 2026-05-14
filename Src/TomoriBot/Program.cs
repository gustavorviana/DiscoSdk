using DiscoSdk;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TomoriBot;

var token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN") ?? throw new InvalidOperationException("DISCORD_BOT_TOKEN environment variable is not set.");

using var loggerFactory = LoggerFactory.Create(builder => builder
    .SetMinimumLevel(LogLevel.Trace)
    .AddSimpleConsole(o => { o.SingleLine = true; o.TimestampFormat = "HH:mm:ss "; }));

var dsc = DiscordClientBuilder.Create(token)
    .WithIntents(DiscordIntent.All)
    .WithEventProcessorMaxConcurrency(100)
    .WithLogger(loggerFactory.CreateLogger("DiscoSdk"))
    .WithSlashCommands(Assembly.GetExecutingAssembly())
    .WithContextMenuCommands(Assembly.GetExecutingAssembly())
    // Register message handler (for regular messages)
    .AddEventHandler<MsgTest>()
    // Register application command handler (for slash commands only)
    .AddEventHandler<ApplicationCommandHandler>()
    // Register autocomplete handler (for options with autocomplete: true)
    .AddEventHandler<AutocompleteHandler>()
    // Register modal submit handler (for modal submissions)
    .AddEventHandler<ModalSubmitHandler>()
    // Register component interaction handler (for button clicks, select menus, etc.)
    .AddEventHandler<ComponentInteractionHandler>()
    .Build();

var betaGuild = Snowflake.Parse("773618860875579422");

dsc.CommandsUpdateWindowOpened += (_, container) =>
{
    container.AddGlobal(x => x
            .WithName("test")
            .WithDescription("A test command")
            .WithType(ApplicationCommandType.ChatInput)
            .AddBooleanOption(
                name: "ephemeral",
                description: "An input Boolean",
                required: false
            )
        )
        .AddGlobal(x => x
            .WithName("feedback")
            .WithDescription("Open feedback modal")
            .WithType(ApplicationCommandType.ChatInput)
        )
       .AddGuild(betaGuild, x => x
            .WithName("sdk-test-modal")
            .WithDescription("Test modal send and receive (TextInput).")
            .WithType(ApplicationCommandType.ChatInput)
        )
       .AddGuild(betaGuild, x => x
            .WithName("sdk-test-button")
            .WithDescription("Test button send and receive in message.")
            .WithType(ApplicationCommandType.ChatInput)
        )
       .AddGuild(betaGuild, x => x
            .WithName("sdk-test-select")
            .WithDescription("Test String Select send and receive.")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-label")
            .WithDescription("Test modal Label component (label + child).")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-checkbox")
            .WithDescription("Test modal Checkbox component.")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-checkbox-group")
            .WithDescription("Test modal CheckboxGroup component.")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-radio")
            .WithDescription("Open a modal with a RadioGroup component.")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-file-upload")
            .WithDescription("Open a modal with a FileUpload component.")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-mixed-modal")
            .WithDescription("Open a modal with every supported input type at once.")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-buttons-all")
            .WithDescription("Demo all 5 button styles in one action row.")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-select-user")
            .WithDescription("Demo UserSelect (type 5).")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-select-role")
            .WithDescription("Demo RoleSelect (type 6).")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-select-channel")
            .WithDescription("Demo ChannelSelect (type 8).")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-select-mentionable")
            .WithDescription("Demo MentionableSelect (type 7).")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-components-v2")
            .WithDescription("Demonstrate every Components V2 component type.")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-section-thumb")
            .WithDescription("V2: Section with Thumbnail accessory.")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-section-button")
            .WithDescription("V2: Section with Button accessory.")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-media-gallery")
            .WithDescription("V2: MediaGallery grid of images.")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x
            .WithName("sdk-test-container")
            .WithDescription("V2: Container with accent colour + text + separators.")
            .WithType(ApplicationCommandType.ChatInput)
        )
        .AddGuild(betaGuild, x => x.WithName("shutdown").WithDescription("Shutdown bot"));
};

await dsc.StartAsync();
await dsc.WaitReadyAsync();

Console.WriteLine("Bot is ready!");

await dsc.WaitShutdownAsync();