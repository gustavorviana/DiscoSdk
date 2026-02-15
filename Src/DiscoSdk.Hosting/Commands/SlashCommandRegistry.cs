using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands;

internal class SlashCommandRegistry
    : IDiscoModule,
    IApplicationCommandHandler,
    IAutocompleteHandler
{
    private readonly Dictionary<string, CommandInfo> _commands = [];

    public SlashCommandRegistry(IServiceCollection serviceCollection, Assembly[] assemblies)
    {
        var commands = CommandInfo.GetAll(assemblies);
        foreach (var item in commands)
        {
            _commands[item.Info.Name] = item;
            serviceCollection.AddScoped(item.Type);

            foreach (var autocomplete in item.Autocompletes.Values)
                if (autocomplete.AutocompleteType != item.Type)
                    serviceCollection.AddScoped(autocomplete.AutocompleteType);
        }
    }

    Task IDiscoModule.OnPreInitializeAsync(IDiscordClient discordClient)
    {
        return Task.CompletedTask;
    }

    async Task IDiscoModule.OnGatewayReadyAsync(IDiscordClient discordClient)
    {
        var commandRegistry = discordClient.UpdateCommands();

        foreach (var item in _commands.Values)
        {
            var command = item.GetCommandBuilder().Build();
            if (item.Info.IsGuildCommand())
                foreach (var guildId in item.Info.GuildIds)
                    commandRegistry.AddGuild(Snowflake.Parse(guildId), command);
            else
                commandRegistry.AddGlobal(command);
        }

        await commandRegistry.DeletePrevious().ExecuteAsync();
    }

    Task IDiscoModule.OnShutdownAsync(IDiscordClient discordClient)
    {
        return Task.CompletedTask;
    }

    async Task IDiscordEventHandler<ICommandContext>.HandleAsync(ICommandContext context)
    {
        if (!_commands.TryGetValue(context.Name, out var command))
            return;

        using var scope = context.Client.Services.CreateAsyncScope();
        await command.ExecuteCommandAsync(scope.ServiceProvider, context);
    }

    async Task IDiscordEventHandler<IAutocompleteContext>.HandleAsync(IAutocompleteContext context)
    {
        if (!_commands.TryGetValue(context.CommandName, out var command))
            return;

        using var scope = context.Client.Services.CreateAsyncScope();
        await command.ExecuteAutocompleteAsync(scope.ServiceProvider, context);
    }
}