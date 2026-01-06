using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Rest;

Console.WriteLine("Hello, World!");

await new DiscordClient(new DiscordClientConfig
{
    Intents = DiscoSdk.Hosting.Gateway.GatewayIntent.All,
    Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN") ?? throw new InvalidOperationException("DISCORD_BOT_TOKEN environment variable is not set."),
}).StartAsync();