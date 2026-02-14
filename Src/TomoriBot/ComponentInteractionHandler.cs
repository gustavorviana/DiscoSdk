using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;

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

        switch (componentCustomId)
        {
            case "sdk_test_open_modal":
                await eventData
                    .ReplyModal()
                    .SetCustomId("sdk_test_modal_submit")
                    .SetTitle("SDK Test Modal")
                    .AddActionRow(new TextInputComponent
                    {
                        CustomId = "sdk_test_input",
                        Label = "Field",
                        Placeholder = "Type something"
                    })
                    .ExecuteAsync();
                break;

            case "sdk_test_button":
                Console.WriteLine("[COMPONENT] Button: sdk_test_button");
                await eventData.Reply("Ok").SetEphemeral().ExecuteAsync();
                break;

            case "sdk_test_select":
                var values = interaction.Data.Values;
                Console.WriteLine($"[COMPONENT] Select: sdk_test_select, Values: {(values != null ? string.Join(", ", values) : "null")}");
                await eventData.Reply("Ok").SetEphemeral().ExecuteAsync();
                break;

            case "sdk_test_open_label_modal":
                await eventData
                    .ReplyModal()
                    .SetCustomId("sdk_test_label_submit")
                    .SetTitle("SDK Test Label")
                    .AddActionRow(new TextInputComponent
                    {
                        CustomId = "sdk_test_label_input",
                        Label = "Label with text input",
                        Placeholder = "Type something"
                    })
                    .ExecuteAsync();
                break;

            case "sdk_test_open_checkbox_modal":
                await eventData
                    .ReplyModal()
                    .SetCustomId("sdk_test_checkbox_submit")
                    .SetTitle("SDK Test Checkbox")
                    .AddActionRow(new TextInputComponent
                    {
                        CustomId = "sdk_test_checkbox_agree",
                        Label = "I agree",
                        Placeholder = "yes / no"
                    })
                    .ExecuteAsync();
                break;

            case "sdk_test_open_checkbox_group_modal":
                await eventData
                    .ReplyModal()
                    .SetCustomId("sdk_test_checkbox_group_submit")
                    .SetTitle("SDK Test Checkbox Group")
                    .AddActionRow(
                        new CheckboxGroupBuilder("sdk_test_checkbox_group")
                            .WithLabel("Choose options")
                            .AddOption("opt1", "Option 1")
                            .AddOption("opt2", "Option 2")
                            .AddOption("opt3", "Option 3")
                            .WithMinValues(2)
                            .WithRequired(false))
                    .ExecuteAsync();
                break;

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


