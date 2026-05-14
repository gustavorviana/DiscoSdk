using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using DiscoSdk.Models.Messages.Components;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DiscoSdk.Tests.Models.Messages.Components;

/// <summary>
/// Tests for the modal-exclusive components added or fixed in the audit pass:
/// <see cref="FileUploadComponent"/>, <see cref="RadioGroupComponent"/> (label wrapping fields),
/// and the routing in <see cref="ComponentTypeMapping"/>.
/// </summary>
public class ModalComponentsExtendedTests
{
    private readonly JsonSerializerOptions _options = new();

    [Fact]
    public void FileUpload_SerializesType19()
    {
        var json = JsonSerializer.Serialize(new FileUploadComponent
        {
            CustomId = "files",
            MinValues = 1,
            MaxValues = 3,
        }, _options);

        var obj = JsonNode.Parse(json)!.AsObject();
        Assert.Equal(19, obj["type"]!.GetValue<int>());
        Assert.Equal("files", obj["custom_id"]!.GetValue<string>());
        Assert.Equal(1, obj["min_values"]!.GetValue<int>());
        Assert.Equal(3, obj["max_values"]!.GetValue<int>());
        // Label / Description are [JsonIgnore] — never on the wire (they're for the wrapping Label).
        Assert.Null(obj["label"]);
        Assert.Null(obj["description"]);
    }

    [Fact]
    public void ComponentTypeMapping_RoutesFileUploadToFileUploadComponent()
    {
        var raw = """{"type":19,"custom_id":"x"}""";
        var result = ComponentTypeMapping.Deserialize(ComponentType.FileUpload, raw, _options);
        var fileUpload = Assert.IsType<FileUploadComponent>(result);
        Assert.Equal("x", fileUpload.CustomId);
    }

    [Fact]
    public void ComponentTypeMapping_RoutesRadioGroupToRadioGroupComponent()
    {
        var raw = """{"type":21,"custom_id":"x","options":[{"value":"a","label":"A"},{"value":"b","label":"B"}]}""";
        var result = ComponentTypeMapping.Deserialize(ComponentType.RadioGroup, raw, _options);
        var radio = Assert.IsType<RadioGroupComponent>(result);
        Assert.Equal(2, radio.Options.Length);
    }

    [Fact]
    public void ComponentTypeMapping_RoutesCheckboxToCheckboxComponent()
    {
        var raw = """{"type":23,"custom_id":"x","default":true}""";
        var result = ComponentTypeMapping.Deserialize(ComponentType.Checkbox, raw, _options);
        var cb = Assert.IsType<CheckboxComponent>(result);
        Assert.True(cb.Default);
    }
}
