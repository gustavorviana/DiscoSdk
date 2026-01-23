using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;

namespace TomoriBot;

/// <summary>
/// Handler for Discord component interactions (button clicks, select menus, etc.).
/// This handler is automatically called only for MessageComponent interactions.
/// </summary>
internal class ComponentInteractionHandler : IComponentInteractionHandler
{
    public async Task HandleAsync(IInteractionContext eventData)
    {
        var interaction = eventData.Interaction;

        if (interaction.Data == null)
            return;

        var componentCustomId = interaction.Data.CustomId;
        Console.WriteLine($"[COMPONENT] Component clicked: {componentCustomId}");

        // Handle different button clicks
        switch (componentCustomId)
        {
            case "approve_feedback":
                await eventData.Reply(
                    "✅ **Feedback approved!**\n\nThe feedback has been approved successfully."
                ).SetEphemeral()
                .ExecuteAsync();
                break;

            case "reject_feedback":
                await eventData.Reply(
                    "❌ **Feedback rejected.**\n\nThe feedback has been rejected."
                ).SetEphemeral()
                .ExecuteAsync();
                break;

            case "view_feedback_details":
                await eventData.Reply(
                    "📋 **Feedback Details**\n\nHere are the details of the feedback..."
                ).SetEphemeral()
                .ExecuteAsync();
                break;

            default:
                // Unknown component - still respond to avoid error
                await eventData.Reply(
                    $"Unknown component: {componentCustomId}"
                ).SetEphemeral()
                .ExecuteAsync();
                break;
        }
    }
}


