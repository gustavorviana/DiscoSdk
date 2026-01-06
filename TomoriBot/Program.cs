using DiscoSdk.Hosting;
using DiscoSdk.Models.Commands;
using TomoriBot;

Console.WriteLine("Hello, World!");
var dsc = new DiscordClient(new DiscordClientConfig
{
    Intents = DiscoSdk.Hosting.Gateway.GatewayIntent.All,
    Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN") ?? throw new InvalidOperationException("DISCORD_BOT_TOKEN environment variable is not set."),
});

// Register commands before starting
dsc.GlobalCommands.Add(new ApplicationCommand
{
    Name = "test",
    Description = "A test command",
    Type = 1, // CHAT_INPUT (slash command)
    Options = [
        new ApplicationCommandOption{
            Name = "ephemeral",
            Description = "An input string",
            Type = 3, // STRING
            Required = false,
        }
    ]
});

// Register message handler (for regular messages)
dsc.EventDispatcher.Register(new MsgTest());

// Register interaction handler (for slash commands)
dsc.EventDispatcher.Register(new InteractionHandler());

await dsc.StartAsync();