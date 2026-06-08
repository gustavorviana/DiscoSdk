using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Commands.Callers.Parameters;
using DiscoSdk.Hosting.Commands.Callers.Results;
using DiscoSdk.Hosting.Gateway.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands;

internal class AutoCompleteInfo(Type type, MethodInfo method, string commandName, string optionName) : SlashCommandHandlerCaller
{
    private readonly ParameterCollection _parameters = new([.. method.GetParameters().Select(ParamInfo.Create)]);
    private readonly MethodCaller _method = MethodCaller.From(method);
    public string CommandName { get; } = commandName;
    public string OptionName { get; } = optionName;
    public bool IsFireAndForget => FireAndForgetCache.IsFireAndForget(_method.Method);

    public override Type Type { get; } = type;

    public Task ExecuteAsync(IServiceProvider service, IAutoCompleteContext context, CancellationToken token)
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
                service.GetService<ILogger<AutoCompleteInfo>>()?.Log(
                    LogLevel.Error, ex,
                    "Error in fire-and-forget AutoComplete {Command}/{Option} (exception cannot propagate)",
                    CommandName, OptionName);
            }
        }, token);
        return Task.CompletedTask;
    }

    public static IReadOnlyDictionary<AutoCompleteName, AutoCompleteInfo> GetAll(Type commandClassType)
    {
        var items = new Dictionary<AutoCompleteName, AutoCompleteInfo>();
        GetAllOfAutoCompleteHandlerAttribute(items, commandClassType);
        GetAllOfSlashOptionAttribute(items, commandClassType);

        return items;
    }

    private static void GetAllOfAutoCompleteHandlerAttribute(Dictionary<AutoCompleteName, AutoCompleteInfo> items, Type commandClassType)
    {
        var contextType = typeof(IAutoCompleteContext);
        foreach (var method in commandClassType.GetMethods(CommandReflection.Flags))
        {
            var attribute = method.GetCustomAttribute<AutoCompleteHandlerAttribute>();
            if (attribute == null)
                continue;

            var methodParams = method.GetParameters();
            if (methodParams.Length != 1 || methodParams.First().ParameterType != contextType)
                continue;

            var name = new AutoCompleteName(attribute.CommandName, attribute.OptionName, attribute.Subcommand, attribute.SubcommandGroup);

            if (items.ContainsKey(name))
                throw new InvalidOperationException($"AutoComplete \"{name}\" already exists.");

            items[name] = new AutoCompleteInfo(commandClassType, method, attribute.CommandName, attribute.OptionName);
        }
    }

    private static void GetAllOfSlashOptionAttribute(Dictionary<AutoCompleteName, AutoCompleteInfo> items, Type commandClassType)
    {
        foreach (var method in commandClassType.GetMethods(CommandReflection.Flags))
        {
            var commandName = method.GetCustomAttribute<SlashCommandAttribute>()?.Name;
            if (string.IsNullOrEmpty(commandName))
                continue;

            // Sub-command / sub-command-group routing lives on the slash command method itself,
            // not on SlashOptionAttribute. Read it here so the AutoCompleteName covers the full
            // tree-path of the option.
            var subCommand = method.GetCustomAttribute<SubCommandAttribute>()?.Name;
            var subCommandGroup = method.GetCustomAttribute<SubCommandGroupAttribute>()?.Name;

            // Method-level [SlashOption(...)] entries declare options for the command up-front;
            // any of them may carry AutoCompleteType pointing at a reusable handler class.
            foreach (var attribute in method.GetCustomAttributes<SlashOptionAttribute>())
                TryRegister(items, attribute, commandName, attribute.Name, subCommand, subCommandGroup);

            // Parameter-level [SlashOption(...)] entries map onto the method's typed parameters;
            // the option name falls back to the parameter name when the attribute omits it.
            foreach (var parameter in method.GetParameters())
            {
                var attribute = parameter.GetCustomAttribute<SlashOptionAttribute>();
                if (attribute is null)
                    continue;
                TryRegister(items, attribute, commandName, attribute.Name ?? parameter.Name, subCommand, subCommandGroup);
            }
        }
    }

    private static void TryRegister(
        Dictionary<AutoCompleteName, AutoCompleteInfo> items,
        SlashOptionAttribute attribute,
        string commandName,
        string? optionName,
        string? subCommand,
        string? subCommandGroup)
    {
        if (attribute.AutoCompleteType is null || string.IsNullOrEmpty(optionName))
            return;

        var info = GetOfOption(attribute, commandName, optionName);
        if (info is null)
            return;

        var name = new AutoCompleteName(commandName, optionName, subCommand, subCommandGroup);
        if (items.ContainsKey(name))
            throw new InvalidOperationException($"AutoComplete \"{name}\" already exists.");

        items[name] = info;
    }

    public static AutoCompleteInfo? GetOfOption(SlashOptionAttribute? option, string commandName, string optionName)
    {
        if (option?.AutoCompleteType == null)
            return null;

        var AutoCompleteType = option.AutoCompleteType;
        var isSlashCommandHandler = typeof(SlashCommandHandler).IsAssignableFrom(AutoCompleteType);
        var isAutoComplete = typeof(IAutoComplete).IsAssignableFrom(AutoCompleteType);

        if (!isSlashCommandHandler && !isAutoComplete)
            throw new InvalidOperationException(
                $"Type '{AutoCompleteType.FullName}' must inherit '{typeof(SlashCommandHandler).FullName}' or implement '{typeof(IAutoComplete).FullName}'.");

        return new AutoCompleteInfo(AutoCompleteType,
            ReflectionUtils.FindInterfaceMethod(AutoCompleteType, typeof(IAutoComplete), nameof(IAutoComplete.ExecuteAsync))!,
            commandName,
            optionName
        );
    }
}