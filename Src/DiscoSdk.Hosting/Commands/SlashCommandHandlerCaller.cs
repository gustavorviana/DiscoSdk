using DiscoSdk.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace DiscoSdk.Hosting.Commands;

internal abstract class SlashCommandHandlerCaller
{
    public abstract Type Type { get; }
    protected SlashCommandHandler GetHandler(IServiceProvider service)
    {
        var instance = (SlashCommandHandler)service.GetRequiredService(Type);
        instance.Init(service);
        return instance;
    }
}