using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Requests.Messages;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;
using System.Text.Json;
using System.Text.Json.Nodes;
using Channel = DiscoSdk.Models.Channels.Channel;

namespace DiscoSdk.Hosting.Tests.Examples;

/// <summary>
/// End-to-end tests for the message send path. Each test builds a message via the real SDK
/// surface (TextBasedChannelWrapper.SendMessage(...).AddActionRow(...) etc.), runs ExecuteAsync,
/// captures the <see cref="MessageCreateRequest"/> that lands on the mocked
/// <see cref="IDiscordRestClient"/>, re-serializes it through the same JsonSerializerOptions
/// the production code would use, and asserts the wire-format JSON shape.
/// </summary>
public class ClientMessageIntegrationTests : WrapperTestBase
{
    private readonly Snowflake _channelId = new(100);
    private readonly TextBasedChannelWrapper _channel;
    private MessageCreateRequest? _captured;

    public ClientMessageIntegrationTests()
    {
        _channel = new TextBasedChannelWrapper(Client, new Channel { Id = _channelId, Type = ChannelType.GuildText });

        // Capture the body at call time (not during Received() verification) — this is the
        // canonical NSubstitute pattern and works across versions.
        Http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                _captured = call.ArgAt<object?>(2) as MessageCreateRequest;
                return Task.FromResult(new Message { Author = new User { UserId = new Snowflake(1), Username = "bot" } });
            });
    }

    // ---- helpers -------------------------------------------------------------------------------

    /// <summary>
    /// Verifies one <c>SendAsync&lt;Message&gt;</c> hit the channel-messages route and returns
    /// the captured <see cref="MessageCreateRequest"/> body re-serialized for JSON assertions.
    /// </summary>
    private async Task<JsonObject> CaptureRequestJsonAsync()
    {
        await Http.Received(1).SendAsync<Message>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/messages"),
            HttpMethod.Post,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());

        Assert.NotNull(_captured);
        var json = JsonSerializer.Serialize(_captured, Http.JsonOptions);
        return JsonNode.Parse(json)!.AsObject();
    }

    // ============================================================================================
    // V1 messages
    // ============================================================================================

    [Fact]
    public async Task Send_SingleButton_PostsContentAndActionRowAsync()
    {
        await _channel.SendMessage("hello")
            .AddActionRow(new ButtonBuilder(ButtonStyle.Primary, "btn").WithLabel("Click here"))
            .ExecuteAsync();

        var obj = await CaptureRequestJsonAsync();
        Assert.Equal("hello", obj["content"]!.GetValue<string>());
        var row = obj["components"]!.AsArray()[0]!;
        Assert.Equal(1, row["type"]!.GetValue<int>());
        var btn = row["components"]!.AsArray()[0]!;
        Assert.Equal(2, btn["type"]!.GetValue<int>());
        Assert.Equal(1, btn["style"]!.GetValue<int>());
        Assert.Equal("btn", btn["custom_id"]!.GetValue<string>());
        Assert.Equal("Click here", btn["label"]!.GetValue<string>());
    }

    [Fact]
    public async Task Send_AllButtonStyles_PostsFiveButtonsInOneRowAsync()
    {
        await _channel.SendMessage("buttons")
            .AddActionRow(
                new ButtonBuilder(ButtonStyle.Primary,   "p").WithLabel("Primary").Build(),
                new ButtonBuilder(ButtonStyle.Secondary, "s").WithLabel("Secondary").Build(),
                new ButtonBuilder(ButtonStyle.Success,   "g").WithLabel("Success").Build(),
                new ButtonBuilder(ButtonStyle.Danger,    "d").WithLabel("Danger").Build(),
                ButtonBuilder.Link("https://x.test", "Docs").Build())
            .ExecuteAsync();

        var obj = await CaptureRequestJsonAsync();
        var btns = obj["components"]!.AsArray()[0]!["components"]!.AsArray();
        Assert.Equal(5, btns.Count);
        for (var i = 1; i <= 5; i++)
            Assert.Equal(i, btns[i - 1]!["style"]!.GetValue<int>());
        Assert.Equal("https://x.test", btns[4]!["url"]!.GetValue<string>());
        Assert.Null(btns[4]!["custom_id"]);
    }

    [Fact]
    public async Task Send_StringSelect_PostsSelectWithOptionsAsync()
    {
        await _channel.SendMessage("pick")
            .AddActionRow(new StringSelectBuilder("sel")
                .WithPlaceholder("Choose")
                .AddOption("Alpha", "a", "First")
                .AddOption("Bravo", "b"))
            .ExecuteAsync();

        var obj = await CaptureRequestJsonAsync();
        var sel = obj["components"]!.AsArray()[0]!["components"]!.AsArray()[0]!;
        Assert.Equal(3, sel["type"]!.GetValue<int>());
        Assert.Equal("sel", sel["custom_id"]!.GetValue<string>());
        Assert.Equal("Choose", sel["placeholder"]!.GetValue<string>());
        var options = sel["options"]!.AsArray();
        Assert.Equal(2, options.Count);
        Assert.Equal("First", options[0]!["description"]!.GetValue<string>());
    }

    [Fact]
    public async Task Send_UserSelect_PostsType5Async()
    {
        await _channel.SendMessage("u")
            .AddActionRow(new UserSelectBuilder("us").WithPlaceholder("Pick user"))
            .ExecuteAsync();

        var obj = await CaptureRequestJsonAsync();
        var sel = obj["components"]!.AsArray()[0]!["components"]!.AsArray()[0]!;
        Assert.Equal(5, sel["type"]!.GetValue<int>());
    }

    [Fact]
    public async Task Send_RoleSelect_PostsType6Async()
    {
        await _channel.SendMessage("r")
            .AddActionRow(new RoleSelectBuilder("rs"))
            .ExecuteAsync();

        var obj = await CaptureRequestJsonAsync();
        Assert.Equal(6, obj["components"]!.AsArray()[0]!["components"]!.AsArray()[0]!["type"]!.GetValue<int>());
    }

    [Fact]
    public async Task Send_ChannelSelect_PostsType8WithChannelTypesAsync()
    {
        await _channel.SendMessage("c")
            .AddActionRow(new ChannelSelectBuilder("cs")
                .WithChannelTypes(ChannelType.GuildText, ChannelType.GuildVoice))
            .ExecuteAsync();

        var obj = await CaptureRequestJsonAsync();
        var sel = obj["components"]!.AsArray()[0]!["components"]!.AsArray()[0]!;
        Assert.Equal(8, sel["type"]!.GetValue<int>());
        var types = sel["channel_types"]!.AsArray();
        Assert.Equal((int)ChannelType.GuildText, types[0]!.GetValue<int>());
        Assert.Equal((int)ChannelType.GuildVoice, types[1]!.GetValue<int>());
    }

    [Fact]
    public async Task Send_MentionableSelect_PostsType7Async()
    {
        await _channel.SendMessage("m")
            .AddActionRow(new MentionableSelectBuilder("ms"))
            .ExecuteAsync();

        var obj = await CaptureRequestJsonAsync();
        Assert.Equal(7, obj["components"]!.AsArray()[0]!["components"]!.AsArray()[0]!["type"]!.GetValue<int>());
    }

    // ============================================================================================
    // V2 messages — IS_COMPONENTS_V2 flag must be set automatically
    // ============================================================================================

    [Fact]
    public async Task Send_V2Container_AutoSetsIsComponentV2FlagAsync()
    {
        await _channel.SendMessage()
            .AddComponent(new ContainerBuilder()
                .WithAccentColor(0x5865F2)
                .AddTextDisplay("hello V2")
                .Build())
            .ExecuteAsync();

        var obj = await CaptureRequestJsonAsync();
        // IsComponentV2 = 1 << 15 = 32768
        Assert.Equal(32768, obj["flags"]!.GetValue<int>() & 32768);
        var container = obj["components"]!.AsArray()[0]!;
        Assert.Equal(17, container["type"]!.GetValue<int>());
        Assert.Equal(0x5865F2, container["accent_color"]!.GetValue<int>());
    }

    [Fact]
    public async Task Send_V2SectionThumbnail_PostsSectionWithThumbnailAccessoryAsync()
    {
        await _channel.SendMessage()
            .AddComponent(new SectionBuilder()
                .AddText("With image")
                .WithThumbnail("https://x.test/a.png", "alt")
                .Build())
            .ExecuteAsync();

        var obj = await CaptureRequestJsonAsync();
        var section = obj["components"]!.AsArray()[0]!;
        Assert.Equal(9, section["type"]!.GetValue<int>());
        Assert.Equal(11, section["accessory"]!["type"]!.GetValue<int>());
        Assert.Equal("https://x.test/a.png", section["accessory"]!["media"]!["url"]!.GetValue<string>());
    }

    [Fact]
    public async Task Send_V2SectionButton_PostsSectionWithButtonAccessoryAsync()
    {
        await _channel.SendMessage()
            .AddComponent(new SectionBuilder()
                .AddText("With action")
                .WithButton(new ButtonBuilder(ButtonStyle.Success, "go").WithLabel("Go"))
                .Build())
            .ExecuteAsync();

        var obj = await CaptureRequestJsonAsync();
        var section = obj["components"]!.AsArray()[0]!;
        Assert.Equal(9, section["type"]!.GetValue<int>());
        var accessory = section["accessory"]!;
        Assert.Equal(2, accessory["type"]!.GetValue<int>());
        Assert.Equal(3, accessory["style"]!.GetValue<int>());
        Assert.Equal("go", accessory["custom_id"]!.GetValue<string>());
    }

    [Fact]
    public async Task Send_V2MediaGallery_PostsItemsArrayAsync()
    {
        await _channel.SendMessage()
            .AddComponent(new MediaGalleryBuilder()
                .AddImage("https://x.test/1.png", "first")
                .AddImage("https://x.test/2.png", spoiler: true)
                .Build())
            .ExecuteAsync();

        var obj = await CaptureRequestJsonAsync();
        var gallery = obj["components"]!.AsArray()[0]!;
        Assert.Equal(12, gallery["type"]!.GetValue<int>());
        Assert.Equal(2, gallery["items"]!.AsArray().Count);
        Assert.True(gallery["items"]!.AsArray()[1]!["spoiler"]!.GetValue<bool>());
    }

    [Fact]
    public async Task Send_V2ContainerWithEverything_PostsFullTreeAsync()
    {
        await _channel.SendMessage()
            .AddComponent(new ContainerBuilder()
                .WithAccentColor(0xEB459E)
                .AddTextDisplay("# Header")
                .AddComponent(new SectionBuilder()
                    .AddText("S+T")
                    .WithThumbnail("https://x.test/a.png"))
                .AddSeparator(divider: true, SeparatorSpacing.Large)
                .AddComponent(new MediaGalleryBuilder()
                    .AddImage("https://x.test/1.png")
                    .AddImage("https://x.test/2.png"))
                .AddSeparator(divider: false, SeparatorSpacing.Small)
                .AddComponent(new SectionBuilder()
                    .AddText("S+B")
                    .WithButton(new ButtonBuilder(ButtonStyle.Success, "x").WithLabel("Click")))
                .Build())
            .ExecuteAsync();

        var obj = await CaptureRequestJsonAsync();
        var container = obj["components"]!.AsArray()[0]!;
        Assert.Equal(17, container["type"]!.GetValue<int>());
        Assert.Equal(0xEB459E, container["accent_color"]!.GetValue<int>());
        var children = container["components"]!.AsArray();
        Assert.Equal(10, children[0]!["type"]!.GetValue<int>()); // TextDisplay
        Assert.Equal(9,  children[1]!["type"]!.GetValue<int>()); // Section + Thumb
        Assert.Equal(14, children[2]!["type"]!.GetValue<int>()); // Separator large
        Assert.Equal(2,  children[2]!["spacing"]!.GetValue<int>());
        Assert.Equal(12, children[3]!["type"]!.GetValue<int>()); // MediaGallery
        Assert.Equal(14, children[4]!["type"]!.GetValue<int>()); // Separator small
        Assert.Equal(9,  children[5]!["type"]!.GetValue<int>()); // Section + Button
    }

    // ============================================================================================
    // Validation — local guards before reaching IDiscordRestClient
    // ============================================================================================

    [Fact]
    public async Task Send_V2WithContent_ThrowsLocallyBeforeReachingHttpAsync()
    {
        var action = _channel.SendMessage("plain text")
            .AddComponent(new TextDisplayComponent { Content = "v2 text" });

        await Assert.ThrowsAsync<InvalidOperationException>(() => action.ExecuteAsync());

        // No SendAsync should have been issued — validation aborts before the request flies.
        await Http.DidNotReceive().SendAsync<Message>(
            Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Send_ButtonOverLabelLimit_ThrowsLocallyAsync()
    {
        Assert.Throws<ArgumentException>(() =>
            new ButtonBuilder(ButtonStyle.Primary, "x").WithLabel(new string('a', 81)));

        // ensure nothing was sent (no setup happened)
        await Http.DidNotReceive().SendAsync<Message>(
            Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }
}
