using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Models.Messages;
using System.Text;

namespace TomoriBot;

/// <summary>
/// Handler for modal submissions. Echoes back informative summaries of what the user submitted
/// so the SDK round-trip can be verified visually in Discord.
/// </summary>
internal class ModalSubmitHandler : IModalSubmitHandler
{
    public async Task HandleAsync(IModalContext context, IServiceProvider services)
    {
        Console.WriteLine($"[MODAL] Modal submitted: {context.CustomId}");
        foreach (var option in context.Options)
            Console.WriteLine($"[MODAL]   {option.CustomId} = {option.Value}");

        switch (context.CustomId)
        {
            case "sdk_test_modal_submit":
                await EchoTextInput(context, "sdk_test_input");
                return;

            case "sdk_test_label_submit":
                await EchoTextInput(context, "sdk_test_label_input");
                return;

            case "sdk_test_checkbox_submit":
                await EchoCheckbox(context, "sdk_test_checkbox_agree");
                return;

            case "sdk_test_checkbox_group_submit":
                await EchoMultiValues(context, "sdk_test_checkbox_group", "Selected:");
                return;

            case "sdk_test_radio_submit":
                await EchoRadio(context, "sdk_test_radio");
                return;

            case "sdk_test_file_upload_submit":
                await EchoFileUpload(context, "sdk_test_file_upload");
                return;

            case "sdk_test_mixed_submit":
                await EchoMixed(context);
                return;

            case "feedback_modal":
                var feedback = context.GetOption("feedback_input") ?? "No feedback provided";
                await context.Reply($"✅ **Feedback received!**\n\nThank you for your feedback: {feedback}")
                    .SetEphemeral().ExecuteAsync();
                return;
        }

        // Fallback: unknown modal — dump what we got.
        await context.Reply($"Modal `{context.CustomId}` submitted but no handler is wired.")
            .SetEphemeral().ExecuteAsync();
    }

    private static Task EchoTextInput(IModalContext ctx, string field)
    {
        var value = ctx.GetOption(field) ?? "(empty)";
        return ctx.Reply($"**TextInput**\n`{field}` = ``{value}``").SetEphemeral().ExecuteAsync();
    }

    private static Task EchoCheckbox(IModalContext ctx, string field)
    {
        var v = ctx.GetOption(field);
        var checkedState = string.Equals(v, "true", StringComparison.OrdinalIgnoreCase)
            ? "✅ checked"
            : string.Equals(v, "false", StringComparison.OrdinalIgnoreCase)
                ? "❌ unchecked"
                : "(empty)";
        return ctx.Reply($"**Checkbox**\n`{field}` → {checkedState}").SetEphemeral().ExecuteAsync();
    }

    private static Task EchoRadio(IModalContext ctx, string field)
    {
        var v = ctx.GetOption(field) ?? "(none)";
        return ctx.Reply($"**RadioGroup**\nSelected option: ``{v}``").SetEphemeral().ExecuteAsync();
    }

    private static Task EchoMultiValues(IModalContext ctx, string field, string label)
    {
        var raw = ctx.GetOption(field);
        var list = string.IsNullOrEmpty(raw) ? "(none)" : raw.Replace(",", ", ");
        return ctx.Reply($"**CheckboxGroup**\n{label} ``{list}``").SetEphemeral().ExecuteAsync();
    }

    /// <summary>
    /// FileUpload returns attachment IDs in the field's Values; the matching <see cref="Attachment"/>
    /// records (with filename / size / content-type) come back in <c>Data.Resolved.Attachments</c>,
    /// keyed by ID.
    /// </summary>
    private static async Task EchoFileUpload(IModalContext ctx, string field)
    {
        var ids = (ctx.GetOption(field) ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var resolved = await ctx.Interaction.Data.GetResolved().ExecuteAsync();

        if (ids.Length == 0)
        {
            await ctx.Reply("**FileUpload**\n(no file)").SetEphemeral().ExecuteAsync();
            return;
        }

        var sb = new StringBuilder("**FileUpload**\n");
        foreach (var id in ids)
        {
            if (resolved is not null && resolved.Attachments.TryGetValue(id, out var att))
                sb.AppendLine($"• `{att.Filename}` — **{FormatSize(att.Size)}** ({att.ContentType ?? "unknown"})");
            else
                sb.AppendLine($"• attachment `{id}` (metadata not resolved)");
        }
        await ctx.Reply(sb.ToString()).SetEphemeral().ExecuteAsync();
    }

    private static async Task EchoMixed(IModalContext ctx)
    {
        var text = ctx.GetOption("mixed_text") ?? "(empty)";
        var terms = string.Equals(ctx.GetOption("mixed_terms"), "true", StringComparison.OrdinalIgnoreCase) ? "✅" : "❌";
        var color = ctx.GetOption("mixed_color") ?? "(none)";
        var featuresRaw = ctx.GetOption("mixed_features") ?? "";
        var features = string.IsNullOrEmpty(featuresRaw) ? "(none)" : featuresRaw.Replace(",", ", ");

        var fileIds = (ctx.GetOption("mixed_files") ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var resolved = await ctx.Interaction.Data.GetResolved().ExecuteAsync();
        var fileLine = fileIds.Length == 0
            ? "(none)"
            : string.Join(", ", fileIds.Select(id => resolved is not null && resolved.Attachments.TryGetValue(id, out var att)
                ? $"`{att.Filename}` ({FormatSize(att.Size)})"
                : $"`{id}`"));

        var sb = new StringBuilder("**Mixed modal — submitted values**\n");
        sb.AppendLine($"• Name: ``{text}``");
        sb.AppendLine($"• Accepted terms: {terms}");
        sb.AppendLine($"• Colour: ``{color}``");
        sb.AppendLine($"• Features: ``{features}``");
        sb.AppendLine($"• Files: {fileLine}");

        await ctx.Reply(sb.ToString()).SetEphemeral().ExecuteAsync();
    }

    private static string FormatSize(int? bytes)
    {
        if (bytes is null) return "?";
        double size = bytes.Value;
        string[] units = { "B", "KiB", "MiB", "GiB" };
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }
        return $"{size:0.##} {units[unit]}";
    }
}
