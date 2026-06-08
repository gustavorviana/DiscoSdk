using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Commands.Callers.Parameters;
using DiscoSdk.Hosting.Commands.Callers.Results;
using DiscoSdk.Hosting.Gateway.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands;

internal class AutocompleteInfo(Type type, MethodInfo method, string commandName, string optionName) : SlashCommandHandlerCaller
{
    private readonly ParameterCollection _parameters = new([.. method.GetParameters().Select(ParamInfo.Create)]);
    private readonly MethodCaller _method = MethodCaller.From(method);
    public string CommandName { get; } = commandName;
    public string OptionName { get; } = optionName;
    public bool IsFireAndForget => FireAndForgetCache.IsFireAndForget(_method.Method);

    public override Type Type { get; } = type;

    public Task ExecuteAsync(IServiceProvider service, IAutocompleteContext context, CancellationToken token)
    {
        var instance = GetHandler(service);
        var parameters = _parameters.CreateInstances(service, context, token);

        // [FireAndForget] opt-in.
        if (!FireAndForgetCache.IsFireAndForget(_method.Method))
            return _method.ExecuteAsync(instance, parameters, token);

        _ = Task.Run(async () =>
        {
            try
            {
                await _method.ExecuteAsync(instance, parameters, token);
            }
            catch (Exception ex)
            {
                service.GetService<ILogger<AutocompleteInfo>>()?.Log(
                    LogLevel.Error, ex,
                    "Error in fire-and-forget autocomplete {Command}/{Option} (exception cannot propagate)",
                    CommandName, OptionName);
            }
        }, token);
        return Task.CompletedTask;
    }

    public static IReadOnlyDictionary<AutocompleteName, AutocompleteInfo> GetAll(Type commandClassType)
    {
        var items = new Dictionary<AutocompleteName, AutocompleteInfo>();

        var contextType = typeof(IAutocompleteContext);

        foreach (var method in commandClassType.GetMethods(CommandReflection.Flags))
        {
            var attribute = method.GetCustomAttribute<AutocompleteHandlerAttribute>();
            if (attribute == null)
                continue;

            var methodParams = method.GetParameters();
            if (methodParams.Length != 1 || methodParams.First().ParameterType != contextType)
                continue;

            var name = new AutocompleteName(attribute.CommandName, attribute.OptionName, attribute.Subcommand, attribute.SubcommandGroup);

            if (items.ContainsKey(name))
                throw new InvalidOperationException($"Autocomplete \"{name}\" already exists.");

            items[name] = new AutocompleteInfo(commandClassType, method, attribute.CommandName, attribute.OptionName);
        }

        return items;
    }

    public static AutocompleteInfo? GetOfOption(SlashOptionAttribute? option, string commandName, string optionName)
    {
        if (option?.AutocompleteType == null)
            return null;

        var autocompleteHandlerType = typeof(SlashCommandHandler);

        var autoCompleteType = option.AutocompleteType;

        if (!autocompleteHandlerType.IsAssignableFrom(autoCompleteType))
            throw new InvalidOperationException(
            $"Type '{autoCompleteType.FullName}' must implement or inherit '{autocompleteHandlerType.FullName}'.");

        return new AutocompleteInfo(autoCompleteType,
            ReflectionUtils.FindInterfaceMethod(autoCompleteType, autocompleteHandlerType, nameof(IAutocomplete.ExecuteAsync))!,
            commandName,
            optionName
        );
    }
}