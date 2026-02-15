using DiscoSdk;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Logging;
using DiscoSdk.Logging;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using System.Reflection;
using TomoriBot;

var token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN") ?? throw new InvalidOperationException("DISCORD_BOT_TOKEN environment variable is not set.");

var dsc = DiscordClientBuilder.Create(token)
    .WithIntents(DiscordIntent.All)
    .WithEventProcessorMaxConcurrency(100)
    .WithLogger(new ConsoleLogger(LogLevel.Trace))
    .WithSlashCommands(Assembly.GetExecutingAssembly())
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
        .AddGuild(betaGuild, x => x.WithName("shutdown").WithDescription("Shutdown bot"));
};

await dsc.StartAsync();
await dsc.WaitReadyAsync();

Console.WriteLine("Bot is ready!");

await dsc.WaitShutdownAsync();