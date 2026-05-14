using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Tests.Models.Messages.Components;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DiscoSdk.Tests.Examples;

/// <summary>
/// One serialization test per Discord-supported component type. Each test builds the component
/// (using the SDK's builder where one exists, or the class directly) and asserts the JSON wire
/// shape matches what Discord's docs describe. Reference:
/// https://docs.discord.com/developers/components/reference
/// </summary>
public class AllComponentsSerializationTests
{
    private readonly JsonSerializerOptions _options = new();
    private readonly InteractionComponentConverter _arrayConverter = new();
    private readonly ModalComponentConverter _modalConverter = new();

    private JsonObject SerializeOneMessageComponent(IInteractionComponent c)
    {
        var json = JsonSerializer.Serialize(new[] { c }, _options.WithConverter(_arrayConverter));
        return JsonNode.Parse(json)!.AsArray()[0]!.AsObject();
    }

    private JsonObject SerializeOneModalComponent(IModalComponent c)
    {
        var json = JsonSerializer.Serialize(new[] { c }, _options.WithConverter(_modalConverter));
        return JsonNode.Parse(json)!.AsArray()[0]!.AsObject();
    }

    // ====== V1 — Button (every style) =========================================================

    [Fact]
    public void Button_Primary()
    {
        var btn = new ButtonBuilder(ButtonStyle.Primary, "p").WithLabel("Primary").Build();
        var obj = SerializeOneMessageComponent(btn);
        Assert.Equal(2, obj["type"]!.GetValue<int>());
        Assert.Equal(1, obj["style"]!.GetValue<int>());
        Assert.Equal("p", obj["custom_id"]!.GetValue<string>());
        Assert.Equal("Primary", obj["label"]!.GetValue<string>());
    }

    [Fact]
    public void Button_Secondary()
    {
        var obj = SerializeOneMessageComponent(new ButtonBuilder(ButtonStyle.Secondary, "s").WithLabel("Secondary").Build());
        Assert.Equal(2, obj["style"]!.GetValue<int>());
    }

    [Fact]
    public void Button_Success()
    {
        var obj = SerializeOneMessageComponent(new ButtonBuilder(ButtonStyle.Success, "g").WithLabel("Go").Build());
        Assert.Equal(3, obj["style"]!.GetValue<int>());
    }

    [Fact]
    public void Button_Danger()
    {
        var obj = SerializeOneMessageComponent(new ButtonBuilder(ButtonStyle.Danger, "d").WithLabel("Delete").Build());
        Assert.Equal(4, obj["style"]!.GetValue<int>());
    }

    [Fact]
    public void Button_Link()
    {
        var btn = ButtonBuilder.Link("https://x.test", "Open").Build();
        var obj = SerializeOneMessageComponent(btn);
        Assert.Equal(2, obj["type"]!.GetValue<int>());
        Assert.Equal(5, obj["style"]!.GetValue<int>());
        Assert.Equal("https://x.test", obj["url"]!.GetValue<string>());
        Assert.Equal("Open", obj["label"]!.GetValue<string>());
        Assert.Null(obj["custom_id"]);
    }

    [Fact]
    public void Button_Premium()
    {
        var btn = ButtonBuilder.Premium(new Snowflake(42)).Build();
        var obj = SerializeOneMessageComponent(btn);
        Assert.Equal(2, obj["type"]!.GetValue<int>());
        Assert.Equal(6, obj["style"]!.GetValue<int>());
        Assert.Equal("42", obj["sku_id"]!.GetValue<string>());
        Assert.Null(obj["label"]);
        Assert.Null(obj["custom_id"]);
    }

    [Fact]
    public void Button_WithEmoji_SerializesEmojiObject()
    {
        var btn = new ButtonBuilder(ButtonStyle.Secondary, "x").WithLabel("Yes").WithEmoji(new Emoji { Name = "✅" }).Build();
        var obj = SerializeOneMessageComponent(btn);
        Assert.Equal("✅", obj["emoji"]!["name"]!.GetValue<string>());
    }

    [Fact]
    public void Button_LabelOver80Chars_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new ButtonBuilder(ButtonStyle.Primary, "x").WithLabel(new string('a', 81)).Build());
        Assert.Contains("80", ex.Message);
    }

    // ====== V1 — Select menus ==================================================================

    [Fact]
    public void StringSelect_SerializesOptions()
    {
        var ss = new StringSelectBuilder("pick")
            .WithPlaceholder("Choose")
            .AddOption("A", "a", "First")
            .AddOption("B", "b")
            .Build();

        var obj = SerializeOneMessageComponent(ss);
        Assert.Equal(3, obj["type"]!.GetValue<int>());
        Assert.Equal("pick", obj["custom_id"]!.GetValue<string>());
        Assert.Equal("Choose", obj["placeholder"]!.GetValue<string>());
        var options = obj["options"]!.AsArray();
        Assert.Equal(2, options.Count);
        Assert.Equal("A", options[0]!["label"]!.GetValue<string>());
        Assert.Equal("a", options[0]!["value"]!.GetValue<string>());
        Assert.Equal("First", options[0]!["description"]!.GetValue<string>());
    }

    [Fact]
    public void StringSelect_TooManyOptions_Throws()
    {
        var b = new StringSelectBuilder("x");
        for (var i = 0; i < 25; i++) b.AddOption($"L{i}", $"v{i}");
        Assert.Throws<InvalidOperationException>(() => b.AddOption("overflow", "ov"));
    }

    [Fact]
    public void UserSelect_SerializesType5WithDefault()
    {
        var us = new UserSelectBuilder("pickuser")
            .WithPlaceholder("Pick a user")
            .WithDefaultUser(new Snowflake(111))
            .Build();

        var obj = SerializeOneMessageComponent(us);
        Assert.Equal(5, obj["type"]!.GetValue<int>());
        Assert.Equal("pickuser", obj["custom_id"]!.GetValue<string>());
        var def = obj["default_values"]!.AsArray()[0]!;
        Assert.Equal("111", def["id"]!.GetValue<string>());
        Assert.Equal("user", def["type"]!.GetValue<string>());
    }

    [Fact]
    public void RoleSelect_SerializesType6()
    {
        var obj = SerializeOneMessageComponent(new RoleSelectBuilder("pickrole").Build());
        Assert.Equal(6, obj["type"]!.GetValue<int>());
        Assert.Equal("pickrole", obj["custom_id"]!.GetValue<string>());
    }

    [Fact]
    public void MentionableSelect_SerializesType7()
    {
        var ms = new MentionableSelectBuilder("pickmention")
            .WithDefaultUser(new Snowflake(1))
            .WithDefaultRole(new Snowflake(2))
            .Build();

        var obj = SerializeOneMessageComponent(ms);
        Assert.Equal(7, obj["type"]!.GetValue<int>());
        var defs = obj["default_values"]!.AsArray();
        Assert.Equal("user", defs[0]!["type"]!.GetValue<string>());
        Assert.Equal("role", defs[1]!["type"]!.GetValue<string>());
    }

    [Fact]
    public void ChannelSelect_SerializesType8WithChannelTypes()
    {
        var cs = new ChannelSelectBuilder("pickchan")
            .WithChannelTypes(ChannelType.GuildText, ChannelType.GuildAnnouncement)
            .Build();

        var obj = SerializeOneMessageComponent(cs);
        Assert.Equal(8, obj["type"]!.GetValue<int>());
        var types = obj["channel_types"]!.AsArray();
        Assert.Equal((int)ChannelType.GuildText, types[0]!.GetValue<int>());
        Assert.Equal((int)ChannelType.GuildAnnouncement, types[1]!.GetValue<int>());
    }

    // ====== V1 — ActionRow (message context) ===================================================

    [Fact]
    public void MessageActionRow_WrapsButtons()
    {
        var row = new MessageActionRowComponent
        {
            Components =
            [
                new ButtonBuilder(ButtonStyle.Primary, "a").WithLabel("A").Build(),
                new ButtonBuilder(ButtonStyle.Danger,  "b").WithLabel("B").Build(),
            ],
        };

        var obj = SerializeOneMessageComponent(row);
        Assert.Equal(1, obj["type"]!.GetValue<int>());
        var children = obj["components"]!.AsArray();
        Assert.Equal(2, children.Count);
        Assert.Equal(2, children[0]!["type"]!.GetValue<int>());
        Assert.Equal("a", children[0]!["custom_id"]!.GetValue<string>());
    }

    // ====== V2 — every type ====================================================================

    [Fact]
    public void TextDisplay_SerializesContent()
    {
        var obj = SerializeOneMessageComponent(new TextDisplayComponent { Content = "hi" });
        Assert.Equal(10, obj["type"]!.GetValue<int>());
        Assert.Equal("hi", obj["content"]!.GetValue<string>());
    }

    [Fact]
    public void Section_WithThumbnailAccessory()
    {
        var section = new SectionBuilder()
            .AddText("Hello")
            .WithThumbnail("https://x.test/a.png", "alt")
            .Build();

        var obj = SerializeOneMessageComponent(section);
        Assert.Equal(9, obj["type"]!.GetValue<int>());
        Assert.Equal(10, obj["components"]!.AsArray()[0]!["type"]!.GetValue<int>());
        Assert.Equal(11, obj["accessory"]!["type"]!.GetValue<int>());
        Assert.Equal("https://x.test/a.png", obj["accessory"]!["media"]!["url"]!.GetValue<string>());
    }

    [Fact]
    public void Section_WithButtonAccessory()
    {
        var section = new SectionBuilder()
            .AddText("With a button")
            .WithButton(new ButtonBuilder(ButtonStyle.Success, "go").WithLabel("Go"))
            .Build();

        var obj = SerializeOneMessageComponent(section);
        Assert.Equal(9, obj["type"]!.GetValue<int>());
        Assert.Equal(2, obj["accessory"]!["type"]!.GetValue<int>());
        Assert.Equal(3, obj["accessory"]!["style"]!.GetValue<int>());
    }

    [Fact]
    public void Section_RequiresAccessory()
    {
        Assert.Throws<InvalidOperationException>(() => new SectionBuilder().AddText("x").Build());
    }

    [Fact]
    public void Thumbnail_AsStandaloneSerializes()
    {
        var obj = SerializeOneMessageComponent(new ThumbnailComponent
        {
            Media = new UnfurledMediaItem { Url = "https://x.test/img.png" },
            Spoiler = true,
        });
        Assert.Equal(11, obj["type"]!.GetValue<int>());
        Assert.True(obj["spoiler"]!.GetValue<bool>());
    }

    [Fact]
    public void MediaGallery_SerializesItems()
    {
        var gallery = new MediaGalleryBuilder()
            .AddImage("https://x.test/1.png", "first")
            .AddImage("https://x.test/2.png", spoiler: true)
            .Build();

        var obj = SerializeOneMessageComponent(gallery);
        Assert.Equal(12, obj["type"]!.GetValue<int>());
        var items = obj["items"]!.AsArray();
        Assert.Equal(2, items.Count);
        Assert.Equal("first", items[0]!["description"]!.GetValue<string>());
        Assert.True(items[1]!["spoiler"]!.GetValue<bool>());
    }

    [Fact]
    public void File_SerializesAttachmentUri()
    {
        var obj = SerializeOneMessageComponent(new FileComponent
        {
            File = new UnfurledMediaItem { Url = "attachment://report.pdf" },
        });
        Assert.Equal(13, obj["type"]!.GetValue<int>());
        Assert.Equal("attachment://report.pdf", obj["file"]!["url"]!.GetValue<string>());
    }

    [Fact]
    public void Separator_DefaultsOmittedNullFields()
    {
        var obj = SerializeOneMessageComponent(new SeparatorComponent { Divider = true });
        Assert.Equal(14, obj["type"]!.GetValue<int>());
        Assert.True(obj["divider"]!.GetValue<bool>());
        Assert.Null(obj["spacing"]);
    }

    [Fact]
    public void Separator_LargeSpacing()
    {
        var obj = SerializeOneMessageComponent(new SeparatorComponent { Divider = false, Spacing = SeparatorSpacing.Large });
        Assert.False(obj["divider"]!.GetValue<bool>());
        Assert.Equal(2, obj["spacing"]!.GetValue<int>());
    }

    [Fact]
    public void Container_WithAccentAndChildren()
    {
        var container = new ContainerBuilder()
            .WithAccentColor(0xFF0000)
            .AddTextDisplay("header")
            .AddSeparator()
            .Build();

        var obj = SerializeOneMessageComponent(container);
        Assert.Equal(17, obj["type"]!.GetValue<int>());
        Assert.Equal(0xFF0000, obj["accent_color"]!.GetValue<int>());
        var children = obj["components"]!.AsArray();
        Assert.Equal(10, children[0]!["type"]!.GetValue<int>());
        Assert.Equal(14, children[1]!["type"]!.GetValue<int>());
    }

    // ====== Modal-only components ==============================================================

    [Fact]
    public void TextInput_SerializesType4()
    {
        var input = new TextInputBuilder("name", "Your name", TextInputStyle.Short)
            .WithPlaceholder("Anonymous")
            .WithMinLength(0).WithMaxLength(60)
            .WithRequired(true)
            .Build();

        var obj = SerializeOneModalComponent(new ActionRowComponent { Components = [input] });
        Assert.Equal(1, obj["type"]!.GetValue<int>());
        var inner = obj["components"]!.AsArray()[0]!;
        Assert.Equal(4, inner["type"]!.GetValue<int>());
        Assert.Equal("name", inner["custom_id"]!.GetValue<string>());
        Assert.Equal("Your name", inner["label"]!.GetValue<string>());
        Assert.Equal((int)TextInputStyle.Short, inner["style"]!.GetValue<int>());
        Assert.Equal("Anonymous", inner["placeholder"]!.GetValue<string>());
        Assert.Equal(0, inner["min_length"]!.GetValue<int>());
        Assert.Equal(60, inner["max_length"]!.GetValue<int>());
        Assert.True(inner["required"]!.GetValue<bool>());
    }

    [Fact]
    public void Checkbox_InsideLabel_SerializesType23()
    {
        var checkbox = new CheckboxComponent
        {
            CustomId = "agree",
            Label = "I agree",
            Description = "Tick to accept",
            Default = false,
        };
        var label = new LabelComponent
        {
            Label = checkbox.Label!,
            Description = checkbox.Description,
            Component = checkbox,
        };

        var obj = SerializeOneModalComponent(label);
        Assert.Equal(18, obj["type"]!.GetValue<int>());
        Assert.Equal("I agree", obj["label"]!.GetValue<string>());
        Assert.Equal("Tick to accept", obj["description"]!.GetValue<string>());
        var inner = obj["component"]!;
        Assert.Equal(23, inner["type"]!.GetValue<int>());
        Assert.Equal("agree", inner["custom_id"]!.GetValue<string>());
        Assert.False(inner["default"]!.GetValue<bool>());
    }

    [Fact]
    public void CheckboxGroup_InsideLabel_SerializesType22WithOptions()
    {
        var group = new CheckboxGroupBuilder("features")
            .WithLabel("Pick features")
            .AddOption("a", "Alpha")
            .AddOption("b", "Beta", "Bee")
            .WithMinValues(0)
            .WithRequired(false)
            .Build();
        var label = new LabelComponent { Label = group.Label!, Component = group };

        var obj = SerializeOneModalComponent(label);
        Assert.Equal(18, obj["type"]!.GetValue<int>());
        var inner = obj["component"]!;
        Assert.Equal(22, inner["type"]!.GetValue<int>());
        Assert.Equal("features", inner["custom_id"]!.GetValue<string>());
        Assert.False(inner["required"]!.GetValue<bool>());
        var options = inner["options"]!.AsArray();
        Assert.Equal(2, options.Count);
        Assert.Equal("Alpha", options[0]!["label"]!.GetValue<string>());
        Assert.Equal("Bee", options[1]!["description"]!.GetValue<string>());
    }

    [Fact]
    public void RadioGroup_InsideLabel_SerializesType21WithOptions()
    {
        var group = new RadioGroupBuilder("color")
            .WithLabel("Pick a colour")
            .WithDescription("Single choice")
            .AddOption("red", "Red")
            .AddOption("green", "Green")
            .AddOption("blue", "Blue")
            .Build();
        var label = new LabelComponent
        {
            Label = group.Label!,
            Description = group.Description,
            Component = group,
        };

        var obj = SerializeOneModalComponent(label);
        Assert.Equal(18, obj["type"]!.GetValue<int>());
        Assert.Equal("Single choice", obj["description"]!.GetValue<string>());
        var inner = obj["component"]!;
        Assert.Equal(21, inner["type"]!.GetValue<int>());
        Assert.Equal(3, inner["options"]!.AsArray().Count);
    }

    [Fact]
    public void FileUpload_InsideLabel_SerializesType19WithMinMax()
    {
        var fu = new FileUploadBuilder("files")
            .WithLabel("Upload some files")
            .WithDescription("Up to 3")
            .WithMinValues(1)
            .WithMaxValues(3)
            .WithRequired(true)
            .Build();
        var label = new LabelComponent
        {
            Label = fu.Label!,
            Description = fu.Description,
            Component = fu,
        };

        var obj = SerializeOneModalComponent(label);
        Assert.Equal(18, obj["type"]!.GetValue<int>());
        var inner = obj["component"]!;
        Assert.Equal(19, inner["type"]!.GetValue<int>());
        Assert.Equal("files", inner["custom_id"]!.GetValue<string>());
        Assert.Equal(1, inner["min_values"]!.GetValue<int>());
        Assert.Equal(3, inner["max_values"]!.GetValue<int>());
        Assert.True(inner["required"]!.GetValue<bool>());
    }
}
