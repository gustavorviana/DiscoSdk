using DiscoSdk.Events;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;

namespace TomoriBot;

/// <summary>
/// Handler for Discord application commands (slash commands only).
/// This handler is automatically called only for ApplicationCommand interactions.
/// </summary>
internal class ApplicationCommandHandler : IApplicationCommandHandler
{
    public async Task HandleAsync(IInteractionCreateEvent eventData)
    {
        var interaction = eventData.Interaction;

        if (interaction.Data == null)
            return;

        var commandName = interaction.Data.Name;
        Console.WriteLine($"[INTERACTION] Command received: {commandName}");

        if (commandName == "test")
        {
            var ephemeral = eventData.Interaction.Data?.Options?.FirstOrDefault(o => o.Name == "ephemeral")?.Value is bool b && b;

            var msg = await eventData
                .Reply("This is a test command response.")
                .SetEphemeral(ephemeral)
                .SendAsync(default);

            await Task.Delay(1000);
            await msg
                .Edit()
                .SetContent("This message will self-destruct in 2 seconds...")
                .SendAsync();

                await Task.Delay(2000);
                await msg.DeleteAsync();

            return;
        }

        if (commandName == "feedback")
        {
            var buttons = new MessageComponent[]
            {
                // "Approve" button
                new() {
                    Type = ComponentType.Button,
                    Style = ButtonStyle.Success,
                    Label = "Approve",
                    CustomId = "approve_feedback"
                },
                // "Reject" button
                new()
                {
                    Type = ComponentType.Button,
                    Style = ButtonStyle.Danger,
                    Label = "Reject",
                    CustomId = "reject_feedback"
                },
                // "View Details" button
                new()
                {
                    Type = ComponentType.Button,
                    Style = ButtonStyle.Secondary,
                    Label = "View Details",
                    CustomId = "view_feedback_details"
                }
            };

            await eventData.DeferAsync();

            // Send message with buttons
            await eventData
                .Reply($"✅ **Feedback received!**\n\nWaiting for your action...")
                .SetEphemeral()
                .AddActionRow(buttons)
                .SendAsync();
        }
        else
        {
            // Unknown command - still respond to avoid error
            await eventData.Reply(
                $"Command '{commandName}' not found."
            ).SendAsync();
        }
    }
}

