using DiscoSdk.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace DiscoSdk.Hosting.Commands;

internal abstract class SlashCommandHandlerCaller
{
    public abstract Type Type { get; }
    protected object GetHandler(IServiceProvider service)
    {
        // Prefer DI when registered (scanner does this for assemblies passed to
        // WithSlashCommands). Fall back to ActivatorUtilities so autocompletes declared via
        // [SlashOption(AutoCompleteType = typeof(...))] still get constructor injection even
        // when they live outside scanned assemblies and the host never registered them.
        var instance = service.GetService(Type) ?? ActivatorUtilities.CreateInstance(service, Type);

        // Init only when the instance derives from SlashCommandHandler — pure-IAutocomplete
        // reusable classes don't have it.
        if (instance is SlashCommandHandler slash)
            slash.Init(service);

        return instance;
    }
}