using DiscoSdk;
using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Models.Enums;

namespace TomoriBot.Commands;

/// <summary>
/// Demo command for <see cref="FireAndForgetAttribute"/>. Simulates a slow workload (5 s) without
/// blocking the shard's dispatch worker — the next interaction is processed immediately. The
/// body defers the response first so Discord doesn't time out the 3 s acknowledge window.
/// </summary>
public class SlowQueryCommandHandler : SlashCommandHandler
{
    [SlashCommand("slowquery", "Demo: long-running command (fire-and-forget).", GuildIds = ["773618860875579422"])]
    [SlashOption(SlashCommandOptionType.String, "topic", "Query topic.", required: true)]
    [FireAndForget]
    protected async Task OnExecuteAsync(ICommandContext context)
    {
        var topic = context.GetOption<string>("topic") ?? "unknown";

        await context.Defer().ExecuteAsync();

        await Task.Delay(TimeSpan.FromSeconds(5));

        await context
            .Reply($"Done querying `{topic}`.")
            .SetEphemeral()
            .ExecuteAsync();
    }
}
