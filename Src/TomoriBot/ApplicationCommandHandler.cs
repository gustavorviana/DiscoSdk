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

            await context.Defer(ephemeral).ExecuteAsync();
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

        if (context.Name == "sdk-test-modal")
        {
            var openModalButton = new MessageComponent
            {
                Type = ComponentType.Button,
                Style = ButtonStyle.Primary,
                Label = "Open modal",
                CustomId = "sdk_test_open_modal"
            };
            await context
                .Reply("**SDK Test — Modal** — Click the button to open the modal.")
                .SetEphemeral()
                .AddActionRow(openModalButton)
                .ExecuteAsync();
            return;
        }

        if (context.Name == "sdk-test-button")
        {
            var testButton = new MessageComponent
            {
                Type = ComponentType.Button,
                Style = ButtonStyle.Primary,
                Label = "Click here",
                CustomId = "sdk_test_button"
            };
            await context
                .Reply("**SDK Test — Button** — Click the button to test reception.")
                .SetEphemeral()
                .AddActionRow(testButton)
                .ExecuteAsync();
            return;
        }

        if (context.Name == "sdk-test-select")
        {
            var stringSelect = new MessageComponent
            {
                Type = ComponentType.StringSelect,
                CustomId = "sdk_test_select",
                Placeholder = "Choose an option",
                Options =
                [
                    new SelectOption { Label = "Option A", Value = "opt_a" },
                    new SelectOption { Label = "Option B", Value = "opt_b" },
                    new SelectOption { Label = "Option C", Value = "opt_c" }
                ]
            };
            await context
                .Reply("**SDK Test — Select** — Choose an option to test reception.")
                .SetEphemeral()
                .AddActionRow(stringSelect)
                .ExecuteAsync();
            return;
        }

        if (context.Name == "sdk-test-label")
        {
            var openLabelModalButton = new MessageComponent
            {
                Type = ComponentType.Button,
                Style = ButtonStyle.Primary,
                Label = "Open modal",
                CustomId = "sdk_test_open_label_modal"
            };
            await context
                .Reply("**SDK Test — Label** — Click the button to open a modal with a Label component.")
                .SetEphemeral()
                .AddActionRow(openLabelModalButton)
                .ExecuteAsync();
            return;
        }

        if (context.Name == "sdk-test-checkbox")
        {
            var openCheckboxModalButton = new MessageComponent
            {
                Type = ComponentType.Button,
                Style = ButtonStyle.Primary,
                Label = "Open modal",
                CustomId = "sdk_test_open_checkbox_modal"
            };
            await context
                .Reply("**SDK Test — Checkbox** — Click the button to open a modal with a Checkbox component.")
                .SetEphemeral()
                .AddActionRow(openCheckboxModalButton)
                .ExecuteAsync();
            return;
        }

        if (context.Name == "sdk-test-checkbox-group")
        {
            var openCheckboxGroupModalButton = new MessageComponent
            {
                Type = ComponentType.Button,
                Style = ButtonStyle.Primary,
                Label = "Open modal",
                CustomId = "sdk_test_open_checkbox_group_modal"
            };
            await context
                .Reply("**SDK Test — Checkbox Group** — Click the button to open a modal with a CheckboxGroup.")
                .SetEphemeral()
                .AddActionRow(openCheckboxGroupModalButton)
                .ExecuteAsync();
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

            // Send message with buttons
            await context
                .Reply($"✅ **Feedback received!**\n\nWaiting for your action...")
                .SetEphemeral()
                .AddActionRow(buttons)
                .ExecuteAsync();

            //await context
            //    .ReplyModal()
            //    .SetCustomId("feedback_modal")
            //    .SetTitle("Feedback")
            //    .AddActionRow(new TextInputComponent
            //    {
            //        Label = "Teste",
            //        CustomId = "feedback_input",
            //        Placeholder = "My Placeholder"
            //    })
            //    .ExecuteAsync();

            return;
        }

        if (context.Name == "search")
        {
            var query = context.GetOption<string>("query");
            await context
                .Reply(string.IsNullOrEmpty(query)
                    ? "No search term provided."
                    : $"You selected: **{query}**")
                .SetEphemeral()
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

            _ = context.Client.StopAsync();
            return;
        }

        // Unknown command - still respond to avoid error
        await context.Reply(
            $"Command '{context.Name}' not found."
        ).ExecuteAsync();
    }
}