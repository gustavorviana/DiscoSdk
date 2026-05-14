using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Rest.Models;
using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Rest;
using NSubstitute;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DiscoSdk.Hosting.Tests.Examples;

/// <summary>
/// End-to-end tests for the modal response path. Each test constructs the same modal a
/// component-interaction handler would build, runs <c>ExecuteAsync()</c>, captures the
/// <see cref="InteractionCallbackRequest"/> that lands on the mocked <see cref="IDiscordRestClient"/>,
/// re-serializes it through the same JsonSerializerOptions Discord would receive, and asserts
/// every field — type discriminators, label wrapping, options, min/max, etc.
/// </summary>
public class ClientModalIntegrationTests : WrapperTestBase
{
    private readonly Snowflake _interactionId = new(555);
    private readonly InteractionHandle _handle;
    private InteractionCallbackRequest? _captured;

    public ClientModalIntegrationTests()
    {
        _handle = new InteractionHandle(_interactionId, "tok-abc");

        // Capture the modal callback body at call time. The modal path lands on the non-generic
        // SendAsync (no response body — Discord returns 204).
        Http.SendAsync(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                _captured = call.ArgAt<object?>(2) as InteractionCallbackRequest;
                return Task.CompletedTask;
            });
    }

    // ---- helper ---------------------------------------------------------------------------------

    /// <summary>
    /// Verifies the SDK hit <c>POST /interactions/{id}/{token}/callback</c> with a Modal-type
    /// callback request and returns the inner <see cref="ModalData"/> re-serialized for assertion.
    /// </summary>
    private async Task<JsonObject> CaptureModalDataJsonAsync()
    {
        await Http.Received(1).SendAsync(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"interactions/{_interactionId}/tok-abc/callback"),
            HttpMethod.Post,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());

        Assert.NotNull(_captured);
        Assert.Equal(InteractionCallbackType.Modal, _captured.Type);
        Assert.NotNull(_captured.Data);

        // _captured.Data is a ModalData; round-trip through JsonOptions so the modal-component
        // converter does its thing.
        var json = JsonSerializer.Serialize(_captured.Data, Http.JsonOptions);
        return JsonNode.Parse(json)!.AsObject();
    }

    private ReplyModalRestAction NewModal(string customId, string title)
        => (ReplyModalRestAction)new ReplyModalRestAction(Client, _handle).SetCustomId(customId).SetTitle(title);

    // ============================================================================================
    // /sdk-test-modal — TextInput Short
    // ============================================================================================

    [Fact]
    public async Task ReplyModal_TextInput_LandsAsActionRowWithTextInputAsync()
    {
        await NewModal("submit_text", "TextInput modal")
            .AddActionRow(new TextInputBuilder("field", "Field", TextInputStyle.Short)
                .WithPlaceholder("Type something")
                .WithRequired(true))
            .ExecuteAsync();

        var obj = await CaptureModalDataJsonAsync();
        Assert.Equal("submit_text", obj["custom_id"]!.GetValue<string>());
        Assert.Equal("TextInput modal", obj["title"]!.GetValue<string>());

        var row = obj["components"]!.AsArray()[0]!;
        Assert.Equal(1, row["type"]!.GetValue<int>());                       // ActionRow
        var inner = row["components"]!.AsArray()[0]!;
        Assert.Equal(4, inner["type"]!.GetValue<int>());                      // TextInput
        Assert.Equal("field", inner["custom_id"]!.GetValue<string>());
        Assert.Equal((int)TextInputStyle.Short, inner["style"]!.GetValue<int>());
        Assert.True(inner["required"]!.GetValue<bool>());
    }

    // ============================================================================================
    // /sdk-test-checkbox — single Checkbox (auto-wrapped in Label)
    // ============================================================================================

    [Fact]
    public async Task ReplyModal_Checkbox_LandsAsLabelWrappingCheckboxAsync()
    {
        await NewModal("submit_cb", "Checkbox modal")
            .AddActionRow(new CheckboxComponent
            {
                CustomId = "agree",
                Label = "I agree",
                Description = "Tick to accept",
                Default = false,
            })
            .ExecuteAsync();

        var obj = await CaptureModalDataJsonAsync();
        var label = obj["components"]!.AsArray()[0]!;
        Assert.Equal(18, label["type"]!.GetValue<int>());                     // Label
        Assert.Equal("I agree", label["label"]!.GetValue<string>());
        Assert.Equal("Tick to accept", label["description"]!.GetValue<string>());

        var cb = label["component"]!;
        Assert.Equal(23, cb["type"]!.GetValue<int>());                        // Checkbox
        Assert.Equal("agree", cb["custom_id"]!.GetValue<string>());
        Assert.False(cb["default"]!.GetValue<bool>());
    }

    // ============================================================================================
    // /sdk-test-checkbox-group — CheckboxGroup (auto-wrapped in Label)
    // ============================================================================================

    [Fact]
    public async Task ReplyModal_CheckboxGroup_LandsAsLabelWrappingGroupAsync()
    {
        await NewModal("submit_cbg", "CheckboxGroup modal")
            .AddActionRow(new CheckboxGroupBuilder("features")
                .WithLabel("Choose features")
                .AddOption("a", "Alpha")
                .AddOption("b", "Beta", "Bee")
                .WithMinValues(0)
                .WithRequired(false))
            .ExecuteAsync();

        var obj = await CaptureModalDataJsonAsync();
        var label = obj["components"]!.AsArray()[0]!;
        Assert.Equal(18, label["type"]!.GetValue<int>());
        Assert.Equal("Choose features", label["label"]!.GetValue<string>());

        var cg = label["component"]!;
        Assert.Equal(22, cg["type"]!.GetValue<int>());                        // CheckboxGroup
        Assert.Equal("features", cg["custom_id"]!.GetValue<string>());
        Assert.Equal(0, cg["min_values"]!.GetValue<int>());
        Assert.False(cg["required"]!.GetValue<bool>());
        Assert.Equal(2, cg["options"]!.AsArray().Count);
        Assert.Equal("Bee", cg["options"]!.AsArray()[1]!["description"]!.GetValue<string>());
    }

    // ============================================================================================
    // /sdk-test-radio — RadioGroup (auto-wrapped in Label)
    // ============================================================================================

    [Fact]
    public async Task ReplyModal_RadioGroup_LandsAsLabelWrappingRadioAsync()
    {
        await NewModal("submit_rg", "Radio modal")
            .AddActionRow(new RadioGroupBuilder("color")
                .WithLabel("Pick a colour")
                .WithDescription("Single choice")
                .AddOption("red", "Red")
                .AddOption("green", "Green")
                .AddOption("blue", "Blue"))
            .ExecuteAsync();

        var obj = await CaptureModalDataJsonAsync();
        var label = obj["components"]!.AsArray()[0]!;
        Assert.Equal(18, label["type"]!.GetValue<int>());
        Assert.Equal("Single choice", label["description"]!.GetValue<string>());

        var rg = label["component"]!;
        Assert.Equal(21, rg["type"]!.GetValue<int>());                        // RadioGroup
        Assert.Equal(3, rg["options"]!.AsArray().Count);
        Assert.Equal("green", rg["options"]!.AsArray()[1]!["value"]!.GetValue<string>());
    }

    // ============================================================================================
    // /sdk-test-file-upload — FileUpload (auto-wrapped in Label)
    // ============================================================================================

    [Fact]
    public async Task ReplyModal_FileUpload_LandsAsLabelWrappingFileUploadAsync()
    {
        await NewModal("submit_fu", "FileUpload modal")
            .AddActionRow(new FileUploadBuilder("files")
                .WithLabel("Upload files")
                .WithDescription("Up to 3")
                .WithMinValues(1)
                .WithMaxValues(3)
                .WithRequired(true))
            .ExecuteAsync();

        var obj = await CaptureModalDataJsonAsync();
        var label = obj["components"]!.AsArray()[0]!;
        Assert.Equal(18, label["type"]!.GetValue<int>());

        var fu = label["component"]!;
        Assert.Equal(19, fu["type"]!.GetValue<int>());                        // FileUpload
        Assert.Equal("files", fu["custom_id"]!.GetValue<string>());
        Assert.Equal(1, fu["min_values"]!.GetValue<int>());
        Assert.Equal(3, fu["max_values"]!.GetValue<int>());
        Assert.True(fu["required"]!.GetValue<bool>());
    }

    // ============================================================================================
    // /sdk-test-mixed-modal — every input type in one modal
    // ============================================================================================

    [Fact]
    public async Task ReplyModal_AllInputTypes_LandsAsFiveRowsInOrderAsync()
    {
        await NewModal("submit_mixed", "Mixed inputs")
            .AddActionRow(new TextInputBuilder("text", "Name", TextInputStyle.Short).WithRequired(false))
            .AddActionRow(new CheckboxComponent { CustomId = "agree", Label = "Accept terms", Default = false })
            .AddActionRow(new RadioGroupBuilder("color").WithLabel("Colour")
                .AddOption("r", "Red").AddOption("g", "Green").AddOption("b", "Blue"))
            .AddActionRow(new CheckboxGroupBuilder("features").WithLabel("Features")
                .AddOption("a", "Alpha").AddOption("b", "Beta").WithMinValues(0).WithRequired(false))
            .AddActionRow(new FileUploadBuilder("files").WithLabel("Files").WithMinValues(0).WithMaxValues(1).WithRequired(false))
            .ExecuteAsync();

        var obj = await CaptureModalDataJsonAsync();
        var rows = obj["components"]!.AsArray();
        Assert.Equal(5, rows.Count);

        // Row 0 — ActionRow > TextInput
        Assert.Equal(1, rows[0]!["type"]!.GetValue<int>());
        Assert.Equal(4, rows[0]!["components"]!.AsArray()[0]!["type"]!.GetValue<int>());

        // Row 1 — Label > Checkbox
        Assert.Equal(18, rows[1]!["type"]!.GetValue<int>());
        Assert.Equal(23, rows[1]!["component"]!["type"]!.GetValue<int>());

        // Row 2 — Label > RadioGroup (3 options)
        Assert.Equal(18, rows[2]!["type"]!.GetValue<int>());
        Assert.Equal(21, rows[2]!["component"]!["type"]!.GetValue<int>());
        Assert.Equal(3,  rows[2]!["component"]!["options"]!.AsArray().Count);

        // Row 3 — Label > CheckboxGroup
        Assert.Equal(18, rows[3]!["type"]!.GetValue<int>());
        Assert.Equal(22, rows[3]!["component"]!["type"]!.GetValue<int>());

        // Row 4 — Label > FileUpload
        Assert.Equal(18, rows[4]!["type"]!.GetValue<int>());
        Assert.Equal(19, rows[4]!["component"]!["type"]!.GetValue<int>());
    }

    // ============================================================================================
    // Validation — local guards before reaching the REST client
    // ============================================================================================

    [Fact]
    public async Task ReplyModal_WithoutCustomId_ThrowsBeforeSendingAsync()
    {
        var action = new ReplyModalRestAction(Client, _handle)
            .SetTitle("forgot custom id")
            .AddActionRow(new TextInputBuilder("x", "y", TextInputStyle.Short));

        await Assert.ThrowsAsync<InvalidOperationException>(() => action.ExecuteAsync());
        await Http.DidNotReceive().SendAsync(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReplyModal_RadioWithoutLabel_ThrowsAtAddAsync()
    {
        var modal = new ReplyModalRestAction(Client, _handle).SetCustomId("x").SetTitle("y");
        var radio = new RadioGroupBuilder("r").AddOption("a", "A").AddOption("b", "B").Build();

        // Label-wrapped inputs require a label string at the AddActionRow call.
        Assert.Throws<ArgumentException>(() => modal.AddActionRow(radio));
    }
}
