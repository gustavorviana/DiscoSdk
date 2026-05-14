using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;

namespace TomoriBot;

/// <summary>
/// Handler for component interactions (button clicks, select menu choices). Each
/// <c>sdk_test_open_*_modal</c> button opens the corresponding modal flavour; the other handlers
/// just echo back receipt so the user sees their click landed.
/// </summary>
internal class ComponentInteractionHandler : IComponentInteractionHandler
{
	public async Task HandleAsync(IInteractionContext eventData, IServiceProvider services)
	{
		var interaction = eventData.Interaction;
		if (interaction.Data == null)
			return;

		var customId = interaction.Data.CustomId;
		Console.WriteLine($"[COMPONENT] Component clicked: {customId}");

		switch (customId)
		{
			// ---- Modal-open buttons -----------------------------------------------------
			case "sdk_test_open_modal":
				await eventData.ReplyModal()
					.SetCustomId("sdk_test_modal_submit")
					.SetTitle("SDK Test Modal")
					.AddActionRow(new TextInputBuilder("sdk_test_input", "Field", TextInputStyle.Short)
						.WithPlaceholder("Type something").WithRequired(true))
					.ExecuteAsync();
				break;

			case "sdk_test_open_label_modal":
				await eventData.ReplyModal()
					.SetCustomId("sdk_test_label_submit")
					.SetTitle("SDK Test Label")
					.AddActionRow(new TextInputBuilder("sdk_test_label_input", "Label with text input", TextInputStyle.Paragraph)
						.WithPlaceholder("Type something").WithMaxLength(500))
					.ExecuteAsync();
				break;

			case "sdk_test_open_checkbox_modal":
				await eventData.ReplyModal()
					.SetCustomId("sdk_test_checkbox_submit")
					.SetTitle("SDK Test Checkbox")
					.AddActionRow(new CheckboxComponent
					{
						CustomId = "sdk_test_checkbox_agree",
						Label = "I agree",
						Description = "Tick the box to accept the terms.",
						Default = false,
					})
					.ExecuteAsync();
				break;

			case "sdk_test_open_checkbox_group_modal":
				await eventData.ReplyModal()
					.SetCustomId("sdk_test_checkbox_group_submit")
					.SetTitle("SDK Test Checkbox Group")
					.AddActionRow(new CheckboxGroupBuilder("sdk_test_checkbox_group")
						.WithLabel("Choose options")
						.AddOption("opt1", "Option 1", "Description 1")
						.AddOption("opt2", "Option 2", "Description 2")
						.AddOption("opt3", "Option 3", "Description 3")
						.WithMinValues(1)
						.WithRequired(false))
					.ExecuteAsync();
				break;

			case "sdk_test_open_radio_modal":
				await eventData.ReplyModal()
					.SetCustomId("sdk_test_radio_submit")
					.SetTitle("SDK Test RadioGroup")
					.AddActionRow(new RadioGroupBuilder("sdk_test_radio")
						.WithLabel("Pick exactly one")
						.WithDescription("Single-choice radio set.")
						.AddOption("red",   "Red",   "🔴 Red option")
						.AddOption("green", "Green", "🟢 Green option")
						.AddOption("blue",  "Blue",  "🔵 Blue option"))
					.ExecuteAsync();
				break;

			case "sdk_test_open_file_upload_modal":
				await eventData.ReplyModal()
					.SetCustomId("sdk_test_file_upload_submit")
					.SetTitle("SDK Test FileUpload")
					.AddActionRow(new FileUploadBuilder("sdk_test_file_upload")
						.WithLabel("Upload files")
						.WithDescription("Up to 3 files.")
						.WithMinValues(1)
						.WithMaxValues(3))
					.ExecuteAsync();
				break;

			case "sdk_test_open_mixed_modal":
				// One modal showing every supported input type at once.
				await eventData.ReplyModal()
					.SetCustomId("sdk_test_mixed_submit")
					.SetTitle("Mixed inputs")
					.AddActionRow(new TextInputBuilder("mixed_text", "Your name", TextInputStyle.Short)
						.WithPlaceholder("Anonymous").WithRequired(false).WithMaxLength(60))
					.AddActionRow(new CheckboxComponent
					{
						CustomId = "mixed_terms",
						Label = "I agree to the terms",
						Default = false,
					})
					.AddActionRow(new RadioGroupBuilder("mixed_color")
						.WithLabel("Favorite colour")
						.AddOption("red",   "Red")
						.AddOption("green", "Green")
						.AddOption("blue",  "Blue"))
					.AddActionRow(new CheckboxGroupBuilder("mixed_features")
						.WithLabel("Features you want enabled")
						.AddOption("a", "Alpha")
						.AddOption("b", "Beta")
						.AddOption("c", "Charlie")
						.WithMinValues(0)
						.WithRequired(false))
					.AddActionRow(new FileUploadBuilder("mixed_files")
						.WithLabel("Attach a file (optional)")
						.WithMinValues(0)
						.WithMaxValues(1)
						.WithRequired(false))
					.ExecuteAsync();
				break;

			// ---- Component receipt handlers ---------------------------------------------
			case "sdk_test_button":
				await eventData.Reply("Button received.").SetEphemeral().ExecuteAsync();
				break;

			case "sdk_btn_primary":
			case "sdk_btn_secondary":
			case "sdk_btn_success":
			case "sdk_btn_danger":
				await eventData.Reply($"Button received: `{customId}`.").SetEphemeral().ExecuteAsync();
				break;

			case "sdk_test_v2_button":
				await eventData.Reply("V2 Section accessory button received.").SetEphemeral().ExecuteAsync();
				break;

			case "sdk_test_section_btn":
				await eventData.Reply("Section button accessory received.").SetEphemeral().ExecuteAsync();
				break;

			case "sdk_test_select":
				await eventData.Reply($"StringSelect: `{string.Join(", ", interaction.Data.Values ?? [])}`").SetEphemeral().ExecuteAsync();
				break;

			case "sdk_test_select_user":
			case "sdk_test_select_role":
			case "sdk_test_select_channel":
			case "sdk_test_select_mentionable":
				await eventData.Reply($"{customId} → `{string.Join(", ", interaction.Data.Values ?? [])}`").SetEphemeral().ExecuteAsync();
				break;

			case "approve_feedback":
				await eventData.Reply("✅ **Feedback approved!**").SetEphemeral().ExecuteAsync();
				break;
			case "reject_feedback":
				await eventData.Reply("❌ **Feedback rejected.**").SetEphemeral().ExecuteAsync();
				break;
			case "view_feedback_details":
				await eventData.Reply("📋 **Feedback Details**").SetEphemeral().ExecuteAsync();
				break;

			default:
				await eventData.Reply($"Unknown component: {customId}").SetEphemeral().ExecuteAsync();
				break;
		}
	}
}
