using DiscoSdk.Events;

namespace TomoriBot;

/// <summary>
/// Handler for Discord modal submission interactions.
/// This handler is automatically called only for ModalSubmit interactions.
/// </summary>
internal class ModalSubmitHandler : IModalSubmitHandler
{
    public async Task HandleAsync(IInteractionCreateEvent eventData)
    {
        var interaction = eventData.Interaction;

        if (interaction.Data == null)
            return;

        var modalCustomId = interaction.Data.CustomId;
        Console.WriteLine($"[MODAL] Modal submitted: {modalCustomId}");

        // Example: Handle feedback modal
        if (modalCustomId == "feedback_modal")
        {
            // Get values from modal components
            var feedback = interaction.Data.Components?
                .SelectMany(row => row.Components ?? [])
                .FirstOrDefault(c => c.CustomId == "feedback_input")?
                .Value ?? "No feedback provided";

            Console.WriteLine($"[MODAL] Feedback received: {feedback}");

            // Respond to the modal submission
            await eventData.Reply(
                $"✅ **Feedback received!**\n\nThank you for your feedback: {feedback}"
            ).SetEphemeral()
            .ExecuteAsync();
        }
    }
}


