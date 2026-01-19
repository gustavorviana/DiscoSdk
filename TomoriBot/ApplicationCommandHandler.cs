using DiscoSdk.Contexts.Interactions;
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
    public async Task HandleAsync(ICommandContext context)
    {
        Console.WriteLine($"[INTERACTION] Command received: {context.Name}");

        if (context.Name == "test")
        {
            var ephemeral = context.GetOption<bool>("ephemeral") ?? true;

            await context.Defer().ExecuteAsync();
            var msg = await context
                .Reply($"This is a test command response in the {context.Interaction.Channel.Name} channel.")
                .SetEphemeral(ephemeral)
                .ExecuteAsync(default);

            await Task.Delay(1000);
            const int TIMEOUT = 5;
            for (int i = 0; i < TIMEOUT; i++)
            {
                await msg
                    .Edit()
                    .SetContent($"This message will self-destruct in {TIMEOUT - i} seconds...")
                    .ExecuteAsync();

                await Task.Delay(1000);
            }

            await msg.Delete().ExecuteAsync();

            return;
        }

        if (context.Name == "feedback")
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

            await context.Defer().ExecuteAsync();

            // Send message with buttons
            await context
                .Reply($"✅ **Feedback received!**\n\nWaiting for your action...")
                .SetEphemeral()
                .AddActionRow(buttons)
                .ExecuteAsync();

            return;
        }

        if (context.Name == "status")
        {
            var status = context.GetOption<OnlineStatus>("status");
            if (status == null)
            {
                await context
                    .Reply("Invalid option")
                    .SetEphemeral()
                    .ExecuteAsync();
                return;
            }

            await context
                .Client
                .UpdatePresence()
                .SetStatus(status.Value)
                .ExecuteAsync();

            await context.Reply("Ok").SetEphemeral().ExecuteAsync();

            return;
        }

        if (context.Name == "shutdown")
        {
            await context
                .Reply("Shutdowning...")
                .SetEphemeral()
                .ExecuteAsync();

            await context
                .Client
                .UpdatePresence()
                .SetStatus(OnlineStatus.Invisible)
                .ExecuteAsync();

            await Task.Delay(2000);

            context.Client.StopAsync();
            return;
        }

        // Unknown command - still respond to avoid error
        await context.Reply(
            $"Command '{context.Name}' not found."
        ).ExecuteAsync();
    }
}