using DiscoSdk.Models;

namespace DiscoSdk.Contexts;

public interface IUserContext : IContext
{
    IUser User { get; }
}