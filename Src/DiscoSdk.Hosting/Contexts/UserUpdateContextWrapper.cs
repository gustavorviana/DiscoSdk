using DiscoSdk.Contexts;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts;

internal class UserUpdateContextWrapper(DiscordClient client, IUser user) : ContextWrapper(client), IUserUpdateContext
{
	public IUser User => user;
}
