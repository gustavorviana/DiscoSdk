using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Requests.Applications;
using DiscoSdk.Rest.Actions;
using DiscoSdk.Hosting.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

/// <summary>
/// Concrete implementation of <see cref="IApplicationEmojis"/>. Delegates to
/// <see cref="ApplicationClient"/>.
/// </summary>
internal sealed class ApplicationEmojisSurface(DiscordClient client) : IApplicationEmojis
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    /// <inheritdoc />
    public IRestAction<IReadOnlyList<IEmoji>> List()
        => RestAction<IReadOnlyList<IEmoji>>.Create(async ct =>
        {
            var envelope = await _client.ApplicationClient.ListApplicationEmojisAsync(_client.RequireApplicationId(), ct);
            return envelope.Items.Select(e => (IEmoji)new ApplicationEmojiWrapper(_client, e)).ToList().AsReadOnly();
        });

    /// <inheritdoc />
    public IRestAction<IEmoji> Get(Snowflake emojiId)
        => RestAction<IEmoji>.Create(async ct =>
            new ApplicationEmojiWrapper(_client, await _client.ApplicationClient.GetApplicationEmojiAsync(_client.RequireApplicationId(), emojiId, ct)));

    /// <inheritdoc />
    public IRestAction<IEmoji> Create(string name, DiscordImageBuffer image)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(image);

        return RestAction<IEmoji>.Create(async ct =>
        {
            var imageDataUri = $"data:{image.ImageType};base64,{image.ToBase64()}";
            var request = new CreateApplicationEmojiRequest
            {
                Name = name,
                Image = imageDataUri,
            };
            var emoji = await _client.ApplicationClient.CreateApplicationEmojiAsync(_client.RequireApplicationId(), request, ct);
            return new ApplicationEmojiWrapper(_client, emoji);
        });
    }
}
