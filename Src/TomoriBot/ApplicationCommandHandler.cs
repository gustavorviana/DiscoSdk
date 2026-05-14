using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;

namespace TomoriBot;

/// <summary>
/// Handler for slash commands. Each <c>sdk-test-*</c> command exercises a specific component or
/// modal-input type so we can smoke-test the SDK end-to-end inside a real Discord client.
/// </summary>
internal class ApplicationCommandHandler : IApplicationCommandHandler
{
    public async Task HandleAsync(ICommandContext context, IServiceProvider services)
    {
        Console.WriteLine($"[INTERACTION] Command received: {context.Name}");

        switch (context.Name)
        {
            case "test":                       await RunTest(context); return;
            case "shutdown":                   await RunShutdown(context); return;

            // ---- Modal demos (each one posts a V2 prompt with a button that opens a modal) ----
            case "sdk-test-modal":             await PromptModal(context, "Modal — TextInput", "Open a modal with a TextInput.", "sdk_test_open_modal", 0x5865F2); return;
            case "sdk-test-label":             await PromptModal(context, "Modal — Label", "Modal with a Label wrapping a TextInput.", "sdk_test_open_label_modal", 0x57F287); return;
            case "sdk-test-checkbox":          await PromptModal(context, "Modal — Checkbox", "Modal with a single Checkbox (auto-wrapped in Label).", "sdk_test_open_checkbox_modal", 0xFEE75C); return;
            case "sdk-test-checkbox-group":    await PromptModal(context, "Modal — CheckboxGroup", "Modal with a CheckboxGroup of options.", "sdk_test_open_checkbox_group_modal", 0xEB459E); return;
            case "sdk-test-radio":             await PromptModal(context, "Modal — RadioGroup", "Modal with a RadioGroup (single-choice).", "sdk_test_open_radio_modal", 0xED4245); return;
            case "sdk-test-file-upload":       await PromptModal(context, "Modal — FileUpload", "Modal with a file picker input.", "sdk_test_open_file_upload_modal", 0x747F8D); return;
            case "sdk-test-mixed-modal":       await PromptModal(context, "Modal — Mixed inputs", "TextInput + Checkbox + Radio + CheckboxGroup + FileUpload all in one modal.", "sdk_test_open_mixed_modal", 0x000000); return;

            // ---- Message-component demos ----
            case "sdk-test-button":            await RunSdkTestButton(context); return;
            case "sdk-test-buttons-all":       await RunSdkTestAllButtons(context); return;
            case "sdk-test-select":            await RunSdkTestStringSelect(context); return;
            case "sdk-test-select-user":       await RunSdkTestUserSelect(context); return;
            case "sdk-test-select-role":       await RunSdkTestRoleSelect(context); return;
            case "sdk-test-select-channel":    await RunSdkTestChannelSelect(context); return;
            case "sdk-test-select-mentionable":await RunSdkTestMentionableSelect(context); return;

            // ---- Components V2 messages ----
            case "sdk-test-components-v2":     await RunSdkTestComponentsV2(context); return;
            case "sdk-test-section-thumb":     await RunSdkTestSectionThumbnail(context); return;
            case "sdk-test-section-button":    await RunSdkTestSectionButton(context); return;
            case "sdk-test-media-gallery":     await RunSdkTestMediaGallery(context); return;
            case "sdk-test-container":         await RunSdkTestContainer(context); return;

            case "feedback":                   await RunFeedback(context); return;
        }

        await context.Reply($"Command '{context.Name}' not found.").ExecuteAsync();
    }

    // ===== Modal-prompt helper =====================================================================

    /// <summary>Posts a V2 container prompt with an "Open modal" button that fires <paramref name="buttonCustomId"/>.</summary>
    private static Task PromptModal(ICommandContext context, string title, string body, string buttonCustomId, int accent)
        => context.Reply().SetEphemeral()
            .AddComponent(new ContainerBuilder()
                .WithAccentColor(accent)
                .AddComponent(new SectionBuilder()
                    .AddText($"**{title}**\n{body}")
                    .WithButton(new ButtonBuilder(ButtonStyle.Primary, buttonCustomId).WithLabel("Open modal")))
                .Build())
            .ExecuteAsync();

    // ===== Misc =====================================================================================

    private static async Task RunTest(ICommandContext context)
    {
        var ephemeral = context.GetOption<bool>("ephemeral") ?? true;
        await context.Defer(ephemeral).ExecuteAsync();
        var msg = await context
            .Reply($"This is a test command response in the {context.Interaction.Channel.Name} channel.")
            .SetEphemeral(ephemeral)
            .ExecuteAsync(default);

        await Task.Delay(1000);
        const int TIMEOUT = 5;
        for (var i = 0; i < TIMEOUT; i++)
        {
            await msg.Edit().SetContent($"This message will self-destruct in {TIMEOUT - i} seconds...").ExecuteAsync();
            await Task.Delay(1000);
        }
        await msg.Delete().ExecuteAsync();
    }

    private static async Task RunShutdown(ICommandContext context)
    {
        await context.Reply("Shutdowning...").SetEphemeral().ExecuteAsync();
        await context.Client.UpdatePresence().SetStatus(OnlineStatus.Invisible).ExecuteAsync();
        await Task.Delay(2000);
        _ = context.Client.StopAsync();
    }

    // ===== Message: V1 buttons & selects ===========================================================

    private static async Task RunSdkTestButton(ICommandContext context)
    {
        await context.Reply("**SDK Test — Button** — Click the button to test reception.")
            .SetEphemeral()
            .AddActionRow(new ButtonBuilder(ButtonStyle.Primary, "sdk_test_button").WithLabel("Click here"))
            .ExecuteAsync();
    }

    /// <summary>Shows all 5 button styles (no Premium — that needs a real SKU id).</summary>
    private static async Task RunSdkTestAllButtons(ICommandContext context)
    {
        await context.Reply("**SDK Test — Buttons (all styles)** — one of each.")
            .SetEphemeral()
            .AddActionRow(
                new ButtonBuilder(ButtonStyle.Primary,   "sdk_btn_primary").WithLabel("Primary").Build(),
                new ButtonBuilder(ButtonStyle.Secondary, "sdk_btn_secondary").WithLabel("Secondary").Build(),
                new ButtonBuilder(ButtonStyle.Success,   "sdk_btn_success").WithLabel("Success").Build(),
                new ButtonBuilder(ButtonStyle.Danger,    "sdk_btn_danger").WithLabel("Danger").Build(),
                ButtonBuilder.Link("https://discord.com/developers/docs/components/reference", "Docs (Link)").Build())
            .ExecuteAsync();
    }

    private static async Task RunSdkTestStringSelect(ICommandContext context)
    {
        await context.Reply("**SDK Test — StringSelect** — pick an option.")
            .SetEphemeral()
            .AddActionRow(new StringSelectBuilder("sdk_test_select")
                .WithPlaceholder("Choose an option")
                .AddOption("Option A", "opt_a", "First choice")
                .AddOption("Option B", "opt_b", "Second choice")
                .AddOption("Option C", "opt_c", "Third choice"))
            .ExecuteAsync();
    }

    private static async Task RunSdkTestUserSelect(ICommandContext context)
    {
        await context.Reply("**SDK Test — UserSelect** — pick a user.")
            .SetEphemeral()
            .AddActionRow(new UserSelectBuilder("sdk_test_select_user").WithPlaceholder("Pick a user"))
            .ExecuteAsync();
    }

    private static async Task RunSdkTestRoleSelect(ICommandContext context)
    {
        await context.Reply("**SDK Test — RoleSelect** — pick a role.")
            .SetEphemeral()
            .AddActionRow(new RoleSelectBuilder("sdk_test_select_role").WithPlaceholder("Pick a role"))
            .ExecuteAsync();
    }

    private static async Task RunSdkTestChannelSelect(ICommandContext context)
    {
        await context.Reply("**SDK Test — ChannelSelect** — pick a text channel.")
            .SetEphemeral()
            .AddActionRow(new ChannelSelectBuilder("sdk_test_select_channel")
                .WithPlaceholder("Pick a channel")
                .WithChannelTypes(ChannelType.GuildText, ChannelType.GuildAnnouncement))
            .ExecuteAsync();
    }

    private static async Task RunSdkTestMentionableSelect(ICommandContext context)
    {
        await context.Reply("**SDK Test — MentionableSelect** — pick a user or role.")
            .SetEphemeral()
            .AddActionRow(new MentionableSelectBuilder("sdk_test_select_mentionable").WithPlaceholder("Pick anyone"))
            .ExecuteAsync();
    }

    // ===== Message: Components V2 ==================================================================

    /// <summary>Big demo exercising every V2 component type in a single message.</summary>
    private static async Task RunSdkTestComponentsV2(ICommandContext context)
    {
        await context.Reply().SetEphemeral()
            .AddComponent(new ContainerBuilder()
                .WithAccentColor(0x5865F2)
                .AddTextDisplay("# Components V2 demo\nAll the new top-level components in one container.")
                .AddComponent(new SectionBuilder()
                    .AddText("**Section + Thumbnail**\nText on the left, image accessory on the right.")
                    .WithThumbnail("https://cdn.discordapp.com/embed/avatars/0.png", "Default avatar"))
                .AddSeparator(divider: true, SeparatorSpacing.Large)
                .AddTextDisplay("**MediaGallery** — up to 10 images in a grid:")
                .AddComponent(new MediaGalleryBuilder()
                    .AddImage("https://cdn.discordapp.com/embed/avatars/0.png", "Avatar 0")
                    .AddImage("https://cdn.discordapp.com/embed/avatars/1.png", "Avatar 1")
                    .AddImage("https://cdn.discordapp.com/embed/avatars/2.png", "Avatar 2"))
                .AddSeparator(divider: false, SeparatorSpacing.Small)
                .AddComponent(new SectionBuilder()
                    .AddText("**Section + Button accessory** — click to interact.")
                    .WithButton(new ButtonBuilder(ButtonStyle.Success, "sdk_test_v2_button").WithLabel("Click me")))
                .Build())
            .ExecuteAsync();
    }

    private static async Task RunSdkTestSectionThumbnail(ICommandContext context)
    {
        await context.Reply().SetEphemeral()
            .AddComponent(new SectionBuilder()
                .AddText("**SDK Test — Section + Thumbnail**\nA single Section pairing text with an image accessory.")
                .WithThumbnail("https://cdn.discordapp.com/embed/avatars/3.png", "Random avatar")
                .Build())
            .ExecuteAsync();
    }

    private static async Task RunSdkTestSectionButton(ICommandContext context)
    {
        await context.Reply().SetEphemeral()
            .AddComponent(new SectionBuilder()
                .AddText("**SDK Test — Section + Button**\nSection accessory can also be a button.")
                .WithButton(new ButtonBuilder(ButtonStyle.Secondary, "sdk_test_section_btn").WithLabel("Section accessory"))
                .Build())
            .ExecuteAsync();
    }

    private static async Task RunSdkTestMediaGallery(ICommandContext context)
    {
        await context.Reply().SetEphemeral()
            .AddComponent(new MediaGalleryBuilder()
                .AddImage("https://cdn.discordapp.com/embed/avatars/0.png", "Avatar 0")
                .AddImage("https://cdn.discordapp.com/embed/avatars/1.png", "Avatar 1")
                .AddImage("https://cdn.discordapp.com/embed/avatars/2.png", "Avatar 2", spoiler: true)
                .Build())
            .ExecuteAsync();
    }

    private static async Task RunSdkTestContainer(ICommandContext context)
    {
        await context.Reply().SetEphemeral()
            .AddComponent(new ContainerBuilder()
                .WithAccentColor(0xEB459E)
                .AddTextDisplay("**SDK Test — Container**\nAccent colour bar + a few text blocks separated by dividers.")
                .AddSeparator(divider: true, SeparatorSpacing.Small)
                .AddTextDisplay("First paragraph after a small divider.")
                .AddSeparator(divider: false, SeparatorSpacing.Large)
                .AddTextDisplay("Second paragraph after a large spacer (no line).")
                .Build())
            .ExecuteAsync();
    }

    private static async Task RunFeedback(ICommandContext context)
    {
        await context.Reply("✅ **Feedback received!**\n\nWaiting for your action...")
            .SetEphemeral()
            .AddActionRow(
                new ButtonBuilder(ButtonStyle.Success,   "approve_feedback").WithLabel("Approve").Build(),
                new ButtonBuilder(ButtonStyle.Danger,    "reject_feedback").WithLabel("Reject").Build(),
                new ButtonBuilder(ButtonStyle.Secondary, "view_feedback_details").WithLabel("View Details").Build())
            .ExecuteAsync();
    }
}
