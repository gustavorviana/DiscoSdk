using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Hosting.Utils;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Repositories
{
    internal class UserRepository(DiscordClient client)
    {
        private readonly UserClient _userClient = new(client.HttpClient);
        private readonly SnowflakeCollection<IUser> _users = [];

        public IRestAction<IUser?> Get(Snowflake userId)
        {
            if (userId == default)
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            return RestAction<IUser?>.Create(async cancellationToken =>
            {
                if (_users.TryGetValue(userId, out var user))
                    return user;

                var userModel = await _userClient.GetAsync(userId, cancellationToken);
                if (userModel == null)
                    return null;


                user = new UserWrapper(userModel, client);
                _users[userId] = user;
                return user;
            });
        }

        public void UpdateUser(User updatedUser)
        {
            if (_users.TryGetValue(updatedUser.Id, out var user))
                (user as UserWrapper)?.OnUpdate(updatedUser);
            else
                _users[updatedUser.Id] = new UserWrapper(updatedUser, client);
        }
    }
}