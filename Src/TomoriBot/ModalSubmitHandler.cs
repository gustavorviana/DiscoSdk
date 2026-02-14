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
        if (context.CustomId == "sdk_test_modal_submit" ||
            context.CustomId == "sdk_test_label_submit" ||
            context.CustomId == "sdk_test_checkbox_submit" ||
            context.CustomId == "sdk_test_checkbox_group_submit")
        {
            Console.WriteLine($"[MODAL] Modal submitted: {context.CustomId}");
            foreach (var option in context.Options)
                Console.WriteLine($"[MODAL] Field: {option.CustomId} = {option.Value}");
            await context.Reply("Ok").SetEphemeral().ExecuteAsync();
            return;
        }

        Console.WriteLine($"[MODAL] Modal submitted: {context.CustomId}");

        await context.Defer().ExecuteAsync();
        await Task.Delay(2000);

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