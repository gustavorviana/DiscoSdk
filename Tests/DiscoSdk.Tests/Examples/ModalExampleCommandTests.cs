using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Tests.Models.Messages.Components;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DiscoSdk.Tests.Examples;

/// <summary>
/// One replay test per <c>sdk-test-*</c> modal example. Mirrors what
/// <c>ReplyModalRestAction.AddActionRow</c> does (TextInput → ActionRow, every other modal-only
/// input → Label) and asserts the <see cref="ModalData"/> JSON wire shape Discord receives on
/// <c>InteractionResponse type 9 (Modal)</c>.
/// </summary>
public class ModalExampleCommandTests
{
    private readonly JsonSerializerOptions _options = new();

    private JsonObject SerializeModalData(ModalData data)
    {
        var json = JsonSerializer.Serialize(data, _options);
        return JsonNode.Parse(json)!.AsObject();
    }

    // ====== /sdk-test-modal — TextInput Short ================================================

    [Fact]
    public void SdkTestModal_SendsActionRowWithSingleTextInput()
    {
        var textInput = new TextInputBuilder("sdk_test_input", "Field", TextInputStyle.Short)
            .WithPlaceholder("Type something").WithRequired(true).Build();
        var modal = new ModalData
        {
            CustomId = "sdk_test_modal_submit",
            Title = "SDK Test Modal",
            Components = [new ActionRowComponent { Components = [textInput] }],
        };

        var obj = SerializeModalData(modal);

        Assert.Equal("sdk_test_modal_submit", obj["custom_id"]!.GetValue<string>());
        Assert.Equal("SDK Test Modal", obj["title"]!.GetValue<string>());

        var row = obj["components"]!.AsArray()[0]!;
        Assert.Equal(1, row["type"]!.GetValue<int>());                       // ActionRow
        var inner = row["components"]!.AsArray()[0]!;
        Assert.Equal(4, inner["type"]!.GetValue<int>());                      // TextInput
        Assert.Equal("sdk_test_input", inner["custom_id"]!.GetValue<string>());
        Assert.Equal((int)TextInputStyle.Short, inner["style"]!.GetValue<int>());
        Assert.Equal("Type something", inner["placeholder"]!.GetValue<string>());
        Assert.True(inner["required"]!.GetValue<bool>());
    }

    // ====== /sdk-test-label — TextInput Paragraph wrapped in Label too =======================

    [Fact]
    public void SdkTestLabel_SendsActionRowWithParagraphTextInput()
    {
        var ti = new TextInputBuilder("sdk_test_label_input", "Label with text input", TextInputStyle.Paragraph)
            .WithPlaceholder("Type something").WithMaxLength(500).Build();
        var modal = new ModalData
        {
            CustomId = "sdk_test_label_submit",
            Title = "SDK Test Label",
            Components = [new ActionRowComponent { Components = [ti] }],
        };

        var obj = SerializeModalData(modal);
        var inner = obj["components"]!.AsArray()[0]!["components"]!.AsArray()[0]!;
        Assert.Equal(4, inner["type"]!.GetValue<int>());
        Assert.Equal((int)TextInputStyle.Paragraph, inner["style"]!.GetValue<int>());
        Assert.Equal(500, inner["max_length"]!.GetValue<int>());
    }

    // ====== /sdk-test-checkbox — Checkbox auto-wrapped in Label ==============================

    [Fact]
    public void SdkTestCheckbox_SendsLabelWrappingCheckboxType23()
    {
        var cb = new CheckboxComponent
        {
            CustomId = "sdk_test_checkbox_agree",
            Label = "I agree",
            Description = "Tick the box to accept the terms.",
            Default = false,
        };
        var modal = new ModalData
        {
            CustomId = "sdk_test_checkbox_submit",
            Title = "SDK Test Checkbox",
            Components = [new LabelComponent { Label = cb.Label!, Description = cb.Description, Component = cb }],
        };

        var obj = SerializeModalData(modal);
        var label = obj["components"]!.AsArray()[0]!;
        Assert.Equal(18, label["type"]!.GetValue<int>());                     // Label
        Assert.Equal("I agree", label["label"]!.GetValue<string>());
        Assert.Equal("Tick the box to accept the terms.", label["description"]!.GetValue<string>());

        var checkbox = label["component"]!;
        Assert.Equal(23, checkbox["type"]!.GetValue<int>());                  // Checkbox
        Assert.Equal("sdk_test_checkbox_agree", checkbox["custom_id"]!.GetValue<string>());
        Assert.False(checkbox["default"]!.GetValue<bool>());
    }

    // ====== /sdk-test-checkbox-group — CheckboxGroup auto-wrapped in Label ===================

    [Fact]
    public void SdkTestCheckboxGroup_SendsLabelWrappingCheckboxGroupType22()
    {
        var group = new CheckboxGroupBuilder("sdk_test_checkbox_group")
            .WithLabel("Choose options")
            .AddOption("opt1", "Option 1", "Description 1")
            .AddOption("opt2", "Option 2", "Description 2")
            .AddOption("opt3", "Option 3", "Description 3")
            .WithMinValues(1)
            .WithRequired(false)
            .Build();
        var modal = new ModalData
        {
            CustomId = "sdk_test_checkbox_group_submit",
            Title = "SDK Test Checkbox Group",
            Components = [new LabelComponent { Label = group.Label!, Component = group }],
        };

        var obj = SerializeModalData(modal);
        var label = obj["components"]!.AsArray()[0]!;
        Assert.Equal(18, label["type"]!.GetValue<int>());
        Assert.Equal("Choose options", label["label"]!.GetValue<string>());

        var cg = label["component"]!;
        Assert.Equal(22, cg["type"]!.GetValue<int>());                        // CheckboxGroup
        Assert.Equal("sdk_test_checkbox_group", cg["custom_id"]!.GetValue<string>());
        Assert.Equal(1, cg["min_values"]!.GetValue<int>());
        Assert.False(cg["required"]!.GetValue<bool>());

        var options = cg["options"]!.AsArray();
        Assert.Equal(3, options.Count);
        Assert.Equal("opt1", options[0]!["value"]!.GetValue<string>());
        Assert.Equal("Description 1", options[0]!["description"]!.GetValue<string>());
    }

    // ====== /sdk-test-radio — RadioGroup auto-wrapped in Label ===============================

    [Fact]
    public void SdkTestRadio_SendsLabelWrappingRadioGroupType21()
    {
        var radio = new RadioGroupBuilder("sdk_test_radio")
            .WithLabel("Pick exactly one")
            .WithDescription("Single-choice radio set.")
            .AddOption("red",   "Red",   "🔴 Red option")
            .AddOption("green", "Green", "🟢 Green option")
            .AddOption("blue",  "Blue",  "🔵 Blue option")
            .Build();
        var modal = new ModalData
        {
            CustomId = "sdk_test_radio_submit",
            Title = "SDK Test RadioGroup",
            Components = [new LabelComponent
            {
                Label = radio.Label!,
                Description = radio.Description,
                Component = radio,
            }],
        };

        var obj = SerializeModalData(modal);
        var label = obj["components"]!.AsArray()[0]!;
        Assert.Equal(18, label["type"]!.GetValue<int>());
        Assert.Equal("Pick exactly one", label["label"]!.GetValue<string>());
        Assert.Equal("Single-choice radio set.", label["description"]!.GetValue<string>());

        var rg = label["component"]!;
        Assert.Equal(21, rg["type"]!.GetValue<int>());                        // RadioGroup
        Assert.Equal(3, rg["options"]!.AsArray().Count);
        Assert.Equal("green", rg["options"]!.AsArray()[1]!["value"]!.GetValue<string>());
    }

    // ====== /sdk-test-file-upload — FileUpload auto-wrapped in Label =========================

    [Fact]
    public void SdkTestFileUpload_SendsLabelWrappingFileUploadType19()
    {
        var fu = new FileUploadBuilder("sdk_test_file_upload")
            .WithLabel("Upload files")
            .WithDescription("Up to 3 files.")
            .WithMinValues(1)
            .WithMaxValues(3)
            .Build();
        var modal = new ModalData
        {
            CustomId = "sdk_test_file_upload_submit",
            Title = "SDK Test FileUpload",
            Components = [new LabelComponent
            {
                Label = fu.Label!,
                Description = fu.Description,
                Component = fu,
            }],
        };

        var obj = SerializeModalData(modal);
        var label = obj["components"]!.AsArray()[0]!;
        Assert.Equal(18, label["type"]!.GetValue<int>());
        Assert.Equal("Up to 3 files.", label["description"]!.GetValue<string>());

        var inner = label["component"]!;
        Assert.Equal(19, inner["type"]!.GetValue<int>());                     // FileUpload
        Assert.Equal("sdk_test_file_upload", inner["custom_id"]!.GetValue<string>());
        Assert.Equal(1, inner["min_values"]!.GetValue<int>());
        Assert.Equal(3, inner["max_values"]!.GetValue<int>());
    }

    // ====== /sdk-test-mixed-modal — every input type in one modal ============================

    [Fact]
    public void SdkTestMixedModal_SendsAllFiveInputTypesInCorrectOrder()
    {
        var text = new TextInputBuilder("mixed_text", "Your name", TextInputStyle.Short)
            .WithPlaceholder("Anonymous").WithRequired(false).WithMaxLength(60).Build();
        var checkbox = new CheckboxComponent
        {
            CustomId = "mixed_terms",
            Label = "I agree to the terms",
            Default = false,
        };
        var radio = new RadioGroupBuilder("mixed_color")
            .WithLabel("Favorite colour")
            .AddOption("red", "Red").AddOption("green", "Green").AddOption("blue", "Blue").Build();
        var checkboxGroup = new CheckboxGroupBuilder("mixed_features")
            .WithLabel("Features you want enabled")
            .AddOption("a", "Alpha").AddOption("b", "Beta").AddOption("c", "Charlie")
            .WithMinValues(0).WithRequired(false).Build();
        var fileUpload = new FileUploadBuilder("mixed_files")
            .WithLabel("Attach a file (optional)")
            .WithMinValues(0).WithMaxValues(1).WithRequired(false).Build();

        var modal = new ModalData
        {
            CustomId = "sdk_test_mixed_submit",
            Title = "Mixed inputs",
            Components =
            [
                new ActionRowComponent { Components = [text] },
                new LabelComponent { Label = checkbox.Label!, Component = checkbox },
                new LabelComponent { Label = radio.Label!, Component = radio },
                new LabelComponent { Label = checkboxGroup.Label!, Component = checkboxGroup },
                new LabelComponent { Label = fileUpload.Label!, Component = fileUpload },
            ],
        };

        var obj = SerializeModalData(modal);
        var rows = obj["components"]!.AsArray();
        Assert.Equal(5, rows.Count);

        // Row 0 — ActionRow > TextInput
        Assert.Equal(1, rows[0]!["type"]!.GetValue<int>());
        Assert.Equal(4, rows[0]!["components"]!.AsArray()[0]!["type"]!.GetValue<int>());
        Assert.Equal("mixed_text", rows[0]!["components"]!.AsArray()[0]!["custom_id"]!.GetValue<string>());

        // Row 1 — Label > Checkbox
        Assert.Equal(18, rows[1]!["type"]!.GetValue<int>());
        Assert.Equal(23, rows[1]!["component"]!["type"]!.GetValue<int>());
        Assert.Equal("mixed_terms", rows[1]!["component"]!["custom_id"]!.GetValue<string>());

        // Row 2 — Label > RadioGroup
        Assert.Equal(18, rows[2]!["type"]!.GetValue<int>());
        Assert.Equal(21, rows[2]!["component"]!["type"]!.GetValue<int>());
        Assert.Equal(3,  rows[2]!["component"]!["options"]!.AsArray().Count);

        // Row 3 — Label > CheckboxGroup
        Assert.Equal(18, rows[3]!["type"]!.GetValue<int>());
        Assert.Equal(22, rows[3]!["component"]!["type"]!.GetValue<int>());
        Assert.Equal(0,  rows[3]!["component"]!["min_values"]!.GetValue<int>());

        // Row 4 — Label > FileUpload
        Assert.Equal(18, rows[4]!["type"]!.GetValue<int>());
        Assert.Equal(19, rows[4]!["component"]!["type"]!.GetValue<int>());
        Assert.Equal(0,  rows[4]!["component"]!["min_values"]!.GetValue<int>());
        Assert.Equal(1,  rows[4]!["component"]!["max_values"]!.GetValue<int>());
    }
}
