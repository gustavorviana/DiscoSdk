using DiscoSdk;
using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Logging;
using DiscoSdk.Logging;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using TomoriBot;

var dsc = new DiscordClient(new DiscordClientConfig
{
	Intents = DiscordIntent.All,
	Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN") ?? throw new InvalidOperationException("DISCORD_BOT_TOKEN environment variable is not set."),
	EventProcessorMaxConcurrency = 100,
	Logger = new ConsoleLogger(LogLevel.Trace)
}, DiscoJson.Create());

await dsc.StartAsync();

await dsc.WaitReadyAsync();

await dsc.UpdateCommands()
    .AddGlobal(x => x
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
    .DeletePrevious()
    .RegisterAsync();

// Register message handler (for regular messages)
dsc.EventRegistry.Add(new MsgTest());

// Register application command handler (for slash commands only)
dsc.EventRegistry.Add(new ApplicationCommandHandler());

// Register modal submit handler (for modal submissions)
dsc.EventRegistry.Add(new ModalSubmitHandler());

// Register component interaction handler (for button clicks, select menus, etc.)
dsc.EventRegistry.Add(new ComponentInteractionHandler());

Console.WriteLine("Bot is ready!");

 await dsc.WaitShutdownAsync();