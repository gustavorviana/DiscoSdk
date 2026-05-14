using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Requests.Messages;
using DiscoSdk.Tests.Models.Messages.Components;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DiscoSdk.Tests.Examples;

/// <summary>
/// One replay test per <c>sdk-test-*</c> message-side example command. Each test reconstructs
/// the exact component graph the command produces, packs it into a <see cref="MessageCreateRequest"/>,
/// serializes the request, and asserts the JSON wire shape Discord receives. Lets us refactor
/// builders / converters with confidence — if the wire shape changes, these break loudly.
/// </summary>
public class MessageExampleCommandTests
{
    private readonly JsonSerializerOptions _options = new();

    private JsonObject SerializeMessageRequest(MessageCreateRequest req)
    {
        var json = JsonSerializer.Serialize(req, _options);
        return JsonNode.Parse(json)!.AsObject();
    }

    // ====== /sdk-test-button — single Primary button =========================================

    [Fact]
    public void SdkTestButton_SendsSinglePrimaryButtonInActionRow()
    {
        var req = new MessageCreateRequest
        {
            Content = "**SDK Test — Button** — Click the button to test reception.",
            Components =
            [
                new MessageActionRowComponent
                {
                    Components = [new ButtonBuilder(ButtonStyle.Primary, "sdk_test_button").WithLabel("Click here").Build()],
                },
            ],
        };

        var obj = SerializeMessageRequest(req);

        Assert.Contains("SDK Test", obj["content"]!.GetValue<string>());
        var row = obj["components"]!.AsArray()[0]!;
        Assert.Equal(1, row["type"]!.GetValue<int>());
        var btn = row["components"]!.AsArray()[0]!;
        Assert.Equal(2, btn["type"]!.GetValue<int>());
        Assert.Equal(1, btn["style"]!.GetValue<int>());
        Assert.Equal("sdk_test_button", btn["custom_id"]!.GetValue<string>());
        Assert.Equal("Click here", btn["label"]!.GetValue<string>());
    }

    // ====== /sdk-test-buttons-all — 5 styles =================================================

    [Fact]
    public void SdkTestButtonsAll_SendsFiveButtonsAllStylesInOneRow()
    {
        var req = new MessageCreateRequest
        {
            Content = "**SDK Test — Buttons (all styles)** — one of each.",
            Components =
            [
                new MessageActionRowComponent
                {
                    Components =
                    [
                        new ButtonBuilder(ButtonStyle.Primary,   "sdk_btn_primary").WithLabel("Primary").Build(),
                        new ButtonBuilder(ButtonStyle.Secondary, "sdk_btn_secondary").WithLabel("Secondary").Build(),
                        new ButtonBuilder(ButtonStyle.Success,   "sdk_btn_success").WithLabel("Success").Build(),
                        new ButtonBuilder(ButtonStyle.Danger,    "sdk_btn_danger").WithLabel("Danger").Build(),
                        ButtonBuilder.Link("https://discord.com/developers/docs/components/reference", "Docs (Link)").Build(),
                    ],
                },
            ],
        };

        var obj = SerializeMessageRequest(req);
        var btns = obj["components"]!.AsArray()[0]!["components"]!.AsArray();
        Assert.Equal(5, btns.Count);
        Assert.Equal(1, btns[0]!["style"]!.GetValue<int>());
        Assert.Equal(2, btns[1]!["style"]!.GetValue<int>());
        Assert.Equal(3, btns[2]!["style"]!.GetValue<int>());
        Assert.Equal(4, btns[3]!["style"]!.GetValue<int>());
        Assert.Equal(5, btns[4]!["style"]!.GetValue<int>());
        Assert.Equal("https://discord.com/developers/docs/components/reference", btns[4]!["url"]!.GetValue<string>());
        Assert.Null(btns[4]!["custom_id"]);
    }

    // ====== /sdk-test-select — string select ==================================================

    [Fact]
    public void SdkTestSelect_SendsStringSelectWithThreeOptions()
    {
        var req = new MessageCreateRequest
        {
            Content = "**SDK Test — StringSelect** — pick an option.",
            Components =
            [
                new MessageActionRowComponent
                {
                    Components =
                    [
                        new StringSelectBuilder("sdk_test_select")
                            .WithPlaceholder("Choose an option")
                            .AddOption("Option A", "opt_a", "First choice")
                            .AddOption("Option B", "opt_b", "Second choice")
                            .AddOption("Option C", "opt_c", "Third choice")
                            .Build(),
                    ],
                },
            ],
        };

        var obj = SerializeMessageRequest(req);
        var sel = obj["components"]!.AsArray()[0]!["components"]!.AsArray()[0]!;
        Assert.Equal(3, sel["type"]!.GetValue<int>());
        Assert.Equal("sdk_test_select", sel["custom_id"]!.GetValue<string>());
        Assert.Equal("Choose an option", sel["placeholder"]!.GetValue<string>());
        var options = sel["options"]!.AsArray();
        Assert.Equal(3, options.Count);
        Assert.Equal("opt_a", options[0]!["value"]!.GetValue<string>());
        Assert.Equal("First choice", options[0]!["description"]!.GetValue<string>());
    }

    // ====== /sdk-test-select-user / -role / -channel / -mentionable ==========================

    [Fact]
    public void SdkTestSelectUser_SendsUserSelectWithPlaceholder()
    {
        var req = WrapInActionRow(new UserSelectBuilder("sdk_test_select_user").WithPlaceholder("Pick a user").Build());
        var obj = SerializeMessageRequest(req);
        var sel = obj["components"]!.AsArray()[0]!["components"]!.AsArray()[0]!;
        Assert.Equal(5, sel["type"]!.GetValue<int>());
        Assert.Equal("sdk_test_select_user", sel["custom_id"]!.GetValue<string>());
    }

    [Fact]
    public void SdkTestSelectRole_SendsRoleSelectType6()
    {
        var req = WrapInActionRow(new RoleSelectBuilder("sdk_test_select_role").WithPlaceholder("Pick a role").Build());
        var obj = SerializeMessageRequest(req);
        var sel = obj["components"]!.AsArray()[0]!["components"]!.AsArray()[0]!;
        Assert.Equal(6, sel["type"]!.GetValue<int>());
        Assert.Equal("sdk_test_select_role", sel["custom_id"]!.GetValue<string>());
    }

    [Fact]
    public void SdkTestSelectChannel_SendsChannelSelectWithTypeFilter()
    {
        var req = WrapInActionRow(new ChannelSelectBuilder("sdk_test_select_channel")
            .WithPlaceholder("Pick a channel")
            .WithChannelTypes(ChannelType.GuildText, ChannelType.GuildAnnouncement)
            .Build());

        var obj = SerializeMessageRequest(req);
        var sel = obj["components"]!.AsArray()[0]!["components"]!.AsArray()[0]!;
        Assert.Equal(8, sel["type"]!.GetValue<int>());
        var types = sel["channel_types"]!.AsArray();
        Assert.Equal(2, types.Count);
        Assert.Equal((int)ChannelType.GuildText, types[0]!.GetValue<int>());
    }

    [Fact]
    public void SdkTestSelectMentionable_SendsMentionableSelectType7()
    {
        var req = WrapInActionRow(new MentionableSelectBuilder("sdk_test_select_mentionable").WithPlaceholder("Pick anyone").Build());
        var obj = SerializeMessageRequest(req);
        var sel = obj["components"]!.AsArray()[0]!["components"]!.AsArray()[0]!;
        Assert.Equal(7, sel["type"]!.GetValue<int>());
    }

    // ====== /sdk-test-components-v2 — full V2 demo ============================================

    [Fact]
    public void SdkTestComponentsV2_SendsContainerWithEveryV2Type()
    {
        var req = new MessageCreateRequest
        {
            Components =
            [
                new ContainerBuilder()
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
                    .Build(),
            ],
        };

        var obj = SerializeMessageRequest(req);
        var container = obj["components"]!.AsArray()[0]!;
        Assert.Equal(17, container["type"]!.GetValue<int>());
        Assert.Equal(0x5865F2, container["accent_color"]!.GetValue<int>());

        var children = container["components"]!.AsArray();
        Assert.Equal(10, children[0]!["type"]!.GetValue<int>());        // TextDisplay header
        Assert.Equal(9,  children[1]!["type"]!.GetValue<int>());        // Section w/ thumb
        Assert.Equal(11, children[1]!["accessory"]!["type"]!.GetValue<int>()); // Thumbnail
        Assert.Equal(14, children[2]!["type"]!.GetValue<int>());        // Separator large
        Assert.Equal(2,  children[2]!["spacing"]!.GetValue<int>());
        Assert.Equal(10, children[3]!["type"]!.GetValue<int>());        // TextDisplay before gallery
        Assert.Equal(12, children[4]!["type"]!.GetValue<int>());        // MediaGallery
        Assert.Equal(3,  children[4]!["items"]!.AsArray().Count);
        Assert.Equal(14, children[5]!["type"]!.GetValue<int>());        // Separator small
        Assert.Equal(9,  children[6]!["type"]!.GetValue<int>());        // Section w/ button
        Assert.Equal(2,  children[6]!["accessory"]!["type"]!.GetValue<int>()); // Button accessory
    }

    // ====== /sdk-test-section-thumb / -button =================================================

    [Fact]
    public void SdkTestSectionThumb_SendsSectionWithThumbnail()
    {
        var req = new MessageCreateRequest
        {
            Components =
            [
                new SectionBuilder()
                    .AddText("**SDK Test — Section + Thumbnail**\nA single Section pairing text with an image accessory.")
                    .WithThumbnail("https://cdn.discordapp.com/embed/avatars/3.png", "Random avatar")
                    .Build(),
            ],
        };

        var obj = SerializeMessageRequest(req);
        var section = obj["components"]!.AsArray()[0]!;
        Assert.Equal(9, section["type"]!.GetValue<int>());
        Assert.Equal(11, section["accessory"]!["type"]!.GetValue<int>());
    }

    [Fact]
    public void SdkTestSectionButton_SendsSectionWithButton()
    {
        var req = new MessageCreateRequest
        {
            Components =
            [
                new SectionBuilder()
                    .AddText("**SDK Test — Section + Button**\nSection accessory can also be a button.")
                    .WithButton(new ButtonBuilder(ButtonStyle.Secondary, "sdk_test_section_btn").WithLabel("Section accessory"))
                    .Build(),
            ],
        };

        var obj = SerializeMessageRequest(req);
        var section = obj["components"]!.AsArray()[0]!;
        Assert.Equal(9, section["type"]!.GetValue<int>());
        Assert.Equal(2, section["accessory"]!["type"]!.GetValue<int>());
        Assert.Equal("sdk_test_section_btn", section["accessory"]!["custom_id"]!.GetValue<string>());
    }

    // ====== /sdk-test-media-gallery ===========================================================

    [Fact]
    public void SdkTestMediaGallery_SendsThreeItemsLastOneSpoilered()
    {
        var req = new MessageCreateRequest
        {
            Components =
            [
                new MediaGalleryBuilder()
                    .AddImage("https://cdn.discordapp.com/embed/avatars/0.png", "Avatar 0")
                    .AddImage("https://cdn.discordapp.com/embed/avatars/1.png", "Avatar 1")
                    .AddImage("https://cdn.discordapp.com/embed/avatars/2.png", "Avatar 2", spoiler: true)
                    .Build(),
            ],
        };

        var obj = SerializeMessageRequest(req);
        var gallery = obj["components"]!.AsArray()[0]!;
        Assert.Equal(12, gallery["type"]!.GetValue<int>());
        var items = gallery["items"]!.AsArray();
        Assert.Equal(3, items.Count);
        Assert.True(items[2]!["spoiler"]!.GetValue<bool>());
    }

    // ====== /sdk-test-container ===============================================================

    [Fact]
    public void SdkTestContainer_SendsContainerWithAccentAndSeparators()
    {
        var req = new MessageCreateRequest
        {
            Components =
            [
                new ContainerBuilder()
                    .WithAccentColor(0xEB459E)
                    .AddTextDisplay("**SDK Test — Container**\nAccent colour bar + a few text blocks separated by dividers.")
                    .AddSeparator(divider: true, SeparatorSpacing.Small)
                    .AddTextDisplay("First paragraph after a small divider.")
                    .AddSeparator(divider: false, SeparatorSpacing.Large)
                    .AddTextDisplay("Second paragraph after a large spacer (no line).")
                    .Build(),
            ],
        };

        var obj = SerializeMessageRequest(req);
        var c = obj["components"]!.AsArray()[0]!;
        Assert.Equal(17, c["type"]!.GetValue<int>());
        Assert.Equal(0xEB459E, c["accent_color"]!.GetValue<int>());
        var kids = c["components"]!.AsArray();
        Assert.Equal(5, kids.Count);
        Assert.Equal(10, kids[0]!["type"]!.GetValue<int>());
        Assert.Equal(14, kids[1]!["type"]!.GetValue<int>());
        Assert.True(kids[1]!["divider"]!.GetValue<bool>());
        Assert.Equal(1, kids[1]!["spacing"]!.GetValue<int>());
        Assert.False(kids[3]!["divider"]!.GetValue<bool>());
        Assert.Equal(2, kids[3]!["spacing"]!.GetValue<int>());
    }

    // ====== Helper ============================================================================

    private static MessageCreateRequest WrapInActionRow(IInteractionComponent inner)
        => new()
        {
            Components = [new MessageActionRowComponent { Components = [inner] }],
        };
}
