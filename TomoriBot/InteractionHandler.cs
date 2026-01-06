using DiscoSdk.Events;
using DiscoSdk.Models.Enums;

namespace TomoriBot;

/// <summary>
/// Handler for Discord interactions (slash commands).
/// </summary>
internal class InteractionHandler : IInteractionCreateHandler
{
    public async Task HandleAsync(IInteractionCreateEvent eventData)
    {
        var interaction = eventData.Interaction;

        // Only handle application commands (slash commands)
        if (interaction.Type != InteractionType.ApplicationCommand) // APPLICATION_COMMAND
            return;

        if (interaction.Data == null)
            return;

        var commandName = interaction.Data.Name;
        Console.WriteLine($"[INTERACTION] Command received: {commandName}");

        // Respond immediately to avoid "application did not respond" error
        // You have 3 seconds to respond, or use DeferAsync() for longer operations
        if (commandName == "test")
        {
            var opt = eventData.Interaction?.Data?.Options?.FirstOrDefault(x => x.Name == "ephemeral");
            var ephemeral = opt?.Value is bool b && b;

            await eventData.DeferAsync(ephemeral: ephemeral);
            await Task.Delay(4000);
            await eventData.RespondAsync(
                "Command /test success!",
                ephemeral: false
            );
        }
        else
        {
            // Unknown command - still respond to avoid error
            await eventData.RespondAsync(
                $"Command '{commandName}' not found.",
                ephemeral: true
            );
        }
    }
}

