using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Commands.Callers.Parameters;
using DiscoSdk.Hosting.Commands.Callers.Results;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands;

internal class AutocompleteInfo(Type type, MethodInfo method, string commandName, string optionName) : SlashCommandHandlerCaller
{
    private readonly ParameterCollection _parameters = new([.. method.GetParameters().Select(ParamInfo.Create)]);
    private readonly MethodCaller _method = MethodCaller.From(method);
    public string CommandName { get; } = commandName;
    public string OptionName { get; } = optionName;

    public override Type Type { get; } = type;

    public Task ExecuteAsync(IServiceProvider service, IAutocompleteContext context, CancellationToken token)
    {
        var instance = GetHandler(service);
        var parameters = _parameters.CreateInstances(service, context, token);
        return _method.ExecuteAsync(instance, parameters, token);
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

            var name = new AutocompleteName(attribute.CommandName, attribute.OptionName);

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