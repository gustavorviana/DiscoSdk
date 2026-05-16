using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Hosting.Models.Requests.Applications;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IEditEmojiAction"/> for application-owned emojis. Discord's
/// modify-application-emoji endpoint only accepts a name change — <c>SetRoles</c> is a no-op
/// because application emojis aren't role-scoped.
/// </summary>
internal sealed class EditApplicationEmojiAction(DiscordClient client, Snowflake emojiId)
	: RestAction<IEmoji>, IEditEmojiAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private readonly Snowflake _emojiId = emojiId;
	private string? _name;

	public IEditEmojiAction SetName(string name)
	{
		_name = name ?? throw new ArgumentNullException(nameof(name));
		return this;
	}

	/// <summary>
	/// Application emojis have no role scoping — Discord ignores this field on the wire. The
	/// method stays on the interface to satisfy <see cref="IEditEmojiAction"/>, but does nothing here.
	/// </summary>
	public IEditEmojiAction SetRoles(params Snowflake[] roleIds) => this;

	public override async Task<IEmoji> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(_name))
			throw new InvalidOperationException("Emoji name is required.");

		var request = new ModifyApplicationEmojiRequest { Name = _name };
		var updated = await _client.ApplicationClient.ModifyApplicationEmojiAsync(
			_client.RequireApplicationId(), _emojiId, request, cancellationToken);

		return new ApplicationEmojiWrapper(_client, updated);
	}
}
