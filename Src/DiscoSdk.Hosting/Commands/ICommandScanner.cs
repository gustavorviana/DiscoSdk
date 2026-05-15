using Microsoft.Extensions.DependencyInjection;

namespace DiscoSdk.Hosting.Commands;

/// <summary>
/// Discovery contract: a scanner inspects handler types (slash or context menu), builds the
/// corresponding entries (<see cref="ApplicationCommand"/> + invocation metadata) and writes
/// them straight into a <see cref="CommandRegistryBuilder"/>. The scanner is transient — used
/// once during <c>WithSlashCommands</c> / <c>WithContextMenuCommands</c> and discarded.
/// </summary>
internal interface ICommandScanner
{
    /// <summary>
    /// Runs discovery and populates <paramref name="builder"/>. The same call registers the
    /// discovered handler types in <paramref name="services"/> so the dispatcher can resolve
    /// them at event time.
    /// </summary>
    void ApplyTo(CommandRegistryBuilder builder, IServiceCollection services);
}
