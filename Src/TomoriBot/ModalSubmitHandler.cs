using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;

namespace TomoriBot;

/// <summary>
/// Handler for Discord modal submission interactions.
/// This handler is automatically called only for ModalSubmit interactions.
/// </summary>
internal class ModalSubmitHandler : IModalSubmitHandler
{
    public async Task HandleAsync(IModalContext context)
    {
        Console.WriteLine($"[MODAL] Modal submitted: {context.CustomId}");

        await context.Defer().ExecuteAsync();
        await Task.Delay(2000);

        // Example: Handle feedback modal
        if (context.CustomId == "feedback_modal")
        {
            // Get values from modal components
            var feedback = context.GetOption("feedback_input") ?? "No feedback provided";

            Console.WriteLine($"[MODAL] Feedback received: {feedback}");

            await context.Reply(
                $"✅ **Feedback received!**\n\nThank you for your feedback: {feedback}"
            ).SetEphemeral()
            .ExecuteAsync();
        }
    }
}