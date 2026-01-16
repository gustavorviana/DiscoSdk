using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper that implements <see cref="IUser"/> for a <see cref="User"/> instance.
/// </summary>
internal class UserWrapper : IUser
{
	private readonly User _user;

	/// <summary>
	/// Initializes a new instance of the <see cref="UserWrapper"/> class.
	/// </summary>
	/// <param name="user">The user instance to wrap.</param>
	public UserWrapper(User user)
	{
		_user = user ?? throw new ArgumentNullException(nameof(user));
	}

	/// <inheritdoc />
	public Snowflake Id => _user.Id;

	/// <inheritdoc />
	public DateTimeOffset CreatedAt => _user.Id.CreatedAt;

	/// <inheritdoc />
	public string EffectiveAvatarUrl
	{
		get
		{
			if (!string.IsNullOrEmpty(_user.Avatar))
			{
				var extension = _user.Avatar.StartsWith("a_") ? "gif" : "png";
				return $"https://cdn.discordapp.com/avatars/{_user.Id}/{_user.Avatar}.{extension}";
			}

			// Default avatar based on discriminator
			var discriminator = int.TryParse(_user.Discriminator, out var disc) ? disc : 0;
			return $"https://cdn.discordapp.com/embed/avatars/{discriminator % 5}.png";
		}
	}

	/// <inheritdoc />
	public ImageProxy EffectiveAvatar => new ImageProxy(EffectiveAvatarUrl);
}