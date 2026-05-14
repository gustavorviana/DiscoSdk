using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using DiscoSdk.Models.Messages.Components;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiscoSdk.Tests.Models.Messages.Components;

/// <summary>
/// Verifies the 7 Components V2 classes serialize to the exact wire shape Discord expects, and
/// round-trip back through <see cref="InteractionComponentConverter"/> as the correct concrete
/// types (no silent fallback to the <see cref="MessageComponent"/> god-class).
/// </summary>
public class ComponentsV2SerializationTests
{
    private readonly JsonSerializerOptions _options = new();
    private readonly InteractionComponentConverter _converter = new();

    private JsonObject SerializeOne(IInteractionComponent c)
    {
        var json = JsonSerializer.Serialize(new[] { c }, _options.WithConverter(_converter));
        return JsonNode.Parse(json)!.AsArray()[0]!.AsObject();
    }

    [Fact]
    public void TextDisplay_SerializesTypeAndContent()
    {
        var obj = SerializeOne(new TextDisplayComponent { Content = "hello" });
        Assert.Equal(10, obj["type"]!.GetValue<int>());
        Assert.Equal("hello", obj["content"]!.GetValue<string>());
    }

    [Fact]
    public void Separator_SerializesDividerAndSpacing()
    {
        var obj = SerializeOne(new SeparatorComponent { Divider = true, Spacing = SeparatorSpacing.Large });
        Assert.Equal(14, obj["type"]!.GetValue<int>());
        Assert.True(obj["divider"]!.GetValue<bool>());
        Assert.Equal(2, obj["spacing"]!.GetValue<int>());
    }

    [Fact]
    public void Thumbnail_SerializesMediaAndDescription()
    {
        var obj = SerializeOne(new ThumbnailComponent
        {
            Media = new UnfurledMediaItem { Url = "https://x.test/a.png" },
            Description = "alt"
        });
        Assert.Equal(11, obj["type"]!.GetValue<int>());
        Assert.Equal("https://x.test/a.png", obj["media"]!["url"]!.GetValue<string>());
        Assert.Equal("alt", obj["description"]!.GetValue<string>());
    }

    [Fact]
    public void MediaGallery_SerializesItemsArray()
    {
        var obj = SerializeOne(new MediaGalleryComponent
        {
            Items =
            [
                new MediaGalleryItem { Media = new UnfurledMediaItem { Url = "https://x.test/1.png" } },
                new MediaGalleryItem { Media = new UnfurledMediaItem { Url = "https://x.test/2.png" }, Spoiler = true }
            ]
        });
        Assert.Equal(12, obj["type"]!.GetValue<int>());
        var items = obj["items"]!.AsArray();
        Assert.Equal(2, items.Count);
        Assert.Equal("https://x.test/1.png", items[0]!["media"]!["url"]!.GetValue<string>());
        Assert.True(items[1]!["spoiler"]!.GetValue<bool>());
    }

    [Fact]
    public void File_SerializesFileUriAndSpoiler()
    {
        var obj = SerializeOne(new FileComponent
        {
            File = new UnfurledMediaItem { Url = "attachment://report.pdf" },
            Spoiler = true
        });
        Assert.Equal(13, obj["type"]!.GetValue<int>());
        Assert.Equal("attachment://report.pdf", obj["file"]!["url"]!.GetValue<string>());
        Assert.True(obj["spoiler"]!.GetValue<bool>());
    }

    [Fact]
    public void Section_SerializesComponentsAndAccessoryByRuntimeType()
    {
        var section = new SectionComponent
        {
            Components = [new TextDisplayComponent { Content = "header" }],
            Accessory = new ThumbnailComponent { Media = new UnfurledMediaItem { Url = "https://x.test/a.png" } }
        };

        var obj = SerializeOne(section);

        Assert.Equal(9, obj["type"]!.GetValue<int>());
        Assert.Equal(10, obj["components"]!.AsArray()[0]!["type"]!.GetValue<int>());
        Assert.Equal("header", obj["components"]!.AsArray()[0]!["content"]!.GetValue<string>());
        // Accessory uses the polymorphic single-component converter so Thumbnail's "media" wins.
        Assert.Equal(11, obj["accessory"]!["type"]!.GetValue<int>());
        Assert.Equal("https://x.test/a.png", obj["accessory"]!["media"]!["url"]!.GetValue<string>());
    }

    [Fact]
    public void Container_SerializesAccentColorAndNestedComponents()
    {
        var container = new ContainerComponent
        {
            AccentColor = 0xFF0000,
            Components =
            [
                new TextDisplayComponent { Content = "in container" },
                new SeparatorComponent { Divider = false }
            ]
        };

        var obj = SerializeOne(container);

        Assert.Equal(17, obj["type"]!.GetValue<int>());
        Assert.Equal(0xFF0000, obj["accent_color"]!.GetValue<int>());
        var children = obj["components"]!.AsArray();
        Assert.Equal(10, children[0]!["type"]!.GetValue<int>());
        Assert.Equal(14, children[1]!["type"]!.GetValue<int>());
    }

    // ---- Round-trip (Read) ----

    [Fact]
    public void Read_TextDisplay_ProducesTextDisplayComponent()
    {
        var json = """[{"type":10,"content":"hi"}]""";
        var result = _converter.Read(json, typeof(IInteractionComponent[]), _options);
        var td = Assert.IsType<TextDisplayComponent>(result![0]);
        Assert.Equal("hi", td.Content);
    }

    [Fact]
    public void Read_Container_ProducesContainerComponentWithTypedChildren()
    {
        var json = """[{"type":17,"accent_color":255,"components":[{"type":10,"content":"x"},{"type":14}]}]""";
        var result = _converter.Read(json, typeof(IInteractionComponent[]), _options);
        var container = Assert.IsType<ContainerComponent>(result![0]);
        Assert.Equal(255, container.AccentColor);
        Assert.IsType<TextDisplayComponent>(container.Components[0]);
        Assert.IsType<SeparatorComponent>(container.Components[1]);
    }

    [Fact]
    public void Read_Section_ProducesSectionWithTypedAccessory()
    {
        var json = """[{"type":9,"components":[{"type":10,"content":"hdr"}],"accessory":{"type":11,"media":{"url":"https://x/y.png"}}}]""";
        var result = _converter.Read(json, typeof(IInteractionComponent[]), _options);
        var section = Assert.IsType<SectionComponent>(result![0]);
        Assert.Equal("hdr", section.Components[0].Content);
        var thumb = Assert.IsType<ThumbnailComponent>(section.Accessory);
        Assert.Equal("https://x/y.png", thumb.Media.Url);
    }

    [Fact]
    public void Read_UnknownType_FallsBackToMessageComponent()
    {
        // Type 99 isn't mapped — falls back to the MessageComponent god-class so the SDK still
        // reads forward-compatible payloads without throwing. (Before V1 routing was symmetric,
        // this test used Button (type 2) as the unknown — now Button routes to ButtonComponent.)
        var json = """[{"type":99}]""";
        var result = _converter.Read(json, typeof(IInteractionComponent[]), _options);
        var mc = Assert.IsType<MessageComponent>(result![0]);
        Assert.Equal((ComponentType)99, mc.Type);
    }
}

internal static class JsonSerializerOptionsExtensions
{
    /// <summary>Helper: returns a clone of <paramref name="options"/> with <paramref name="converter"/> added.</summary>
    public static JsonSerializerOptions WithConverter(this JsonSerializerOptions options, JsonConverter converter)
    {
        var clone = new JsonSerializerOptions(options);
        clone.Converters.Add(converter);
        return clone;
    }
}
