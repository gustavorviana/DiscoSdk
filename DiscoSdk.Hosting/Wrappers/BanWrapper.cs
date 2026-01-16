using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper that implements <see cref="IBan"/> for a <see cref="Ban"/> instance.
/// </summary>
internal class BanWrapper : IBan
{
	private readonly DiscordClient _client;

	public BanWrapper(Ban ban, DiscordClient client)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));

		User = new UserWrapper(ban.User, _client);
		Reason = ban.Reason;
    }

	public string? Reason { get; }
	public IUser User { get; }
}