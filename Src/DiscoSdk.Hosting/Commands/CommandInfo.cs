using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Commands.Callers.Parameters;
using DiscoSdk.Hosting.Commands.Callers.Results;
using DiscoSdk.Models.Enums;
using DiscoSdk.Utils;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands;

internal class CommandInfo : SlashCommandHandlerCaller
{
    private readonly MethodCaller _method;
    private readonly ParameterCollection _parameters;
    private readonly SlashOptionAttribute[]? _methodOptions;
    public SlashCommandAttribute Info { get; }
    public override Type Type => _method.Method.DeclaringType!;

    private CommandInfo(
        SlashCommandAttribute info,
        MethodCaller method,
        ParameterCollection parameters,
        SlashOptionAttribute[]? methodOptions)
    {
        Info = info;
        _method = method;
        _parameters = parameters;
        _methodOptions = methodOptions;
    }

    public AutocompleteInfo[] GetAutocompletes()
    {
        if (_methodOptions != null)
        {
            return _methodOptions
                .Select(o => AutocompleteInfo.GetOfOption(o, Info.Name, o.Name!))
                .Where(a => a != null)
                .ToArray()!;
        }

        return _parameters
            .OfType<SlashParamInfo>()
            .Where(x => x.Autocomplete != null)
            .Select(x => x.Autocomplete)
            .ToArray()!;
    }

    public SlashCommandBuilder GetCommandBuilder(Func<AutocompleteName, bool> hasAutocomplete)
    {
        var builder = new SlashCommandBuilder();

        builder.WithName(Info.Name);
        builder.WithDescription(Info.Description);
        builder.WithType(ApplicationCommandType.ChatInput);

        if (_methodOptions != null)
        {
            foreach (var option in _methodOptions)
            {
                if (option.Name == null)
                    throw new InvalidOperationException(
                        $"Method-level '{nameof(SlashOptionAttribute)}' on command '{Info.Name}' must have a Name.");

                builder.AddOption(new Models.Commands.SlashCommandOption
                {
                    Autocomplete = option.AutocompleteType != null || hasAutocomplete(new AutocompleteName(Info.Name, option.Name)),
                    Name = option.Name,
                    ChannelTypes = option.ChannelTypes,
                    Choices = [.. SlashOptionTypeUtils.GetChoices(_method.Method, option.Name).Select(x => x.ToCommandChoice())],
                    Description = option.Description,
                    MinLength = option.GetMinLength(),
                    MaxLength = option.GetMaxLength(),
                    MinValue = option.MinValue,
                    MaxValue = option.MaxValue,
                    Required = option.Required,
                    Type = option.Type,
                });
            }
        }
        else
        {
            foreach (var parameter in _parameters.OfType<ParamFactoryInfo>())
            {
                if (parameter.Type == null)
                    continue;

                builder.AddOption(new Models.Commands.SlashCommandOption
                {
                    Autocomplete = parameter.Autocomplete != null || hasAutocomplete(new AutocompleteName(Info.Name, parameter.Name)),
                    Name = parameter.Name,
                    ChannelTypes = parameter.Option?.ChannelTypes,
                    Choices = [.. parameter.GetChoices().Select(x => x.ToCommandChoice())],
                    Description = parameter.Option?.Description ?? parameter.Name,
                    MinLength = parameter.Option?.GetMinLength(),
                    MaxLength = parameter.MaxLength,
                    MinValue = parameter.MinValue,
                    MaxValue = parameter.MaxValue,
                    Required = parameter.Required,
                    Type = parameter.Type.Value,
                });
            }
        }

        return builder;
    }

    internal static IEnumerable<CommandInfo> GetAll(Type commandClass)
    {
        if (commandClass.IsAbstract || commandClass.IsInterface || !typeof(SlashCommandHandler).IsAssignableFrom(commandClass))
            yield break;

        var methods = commandClass.GetMethods(CommandReflection.Flags);

        foreach (var method in methods)
        {
            var commandAttribute = method.GetCustomAttribute<SlashCommandAttribute>();
            if (commandAttribute != null)
                yield return Create(method, commandAttribute);
        }
    }

    private static CommandInfo Create(MethodInfo method, SlashCommandAttribute commandClass)
    {
        ArgumentNullException.ThrowIfNull(method);

        if (!typeof(SlashCommandHandler).IsAssignableFrom(method.DeclaringType))
            throw new ArgumentException($"Method must be declared on a type assignable to {nameof(SlashCommandHandler)}.", nameof(method));

        var methodOptions = method.GetCustomAttributes<SlashOptionAttribute>().ToArray();

        var hasMethodOptions = methodOptions.Length > 0;

        var parameters = method.GetParameters();
        var resolvers = new ParamInfo[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            resolvers[i] = hasMethodOptions
                ? ParamInfo.Create(param)
                : ParamFactoryInfo.CreateParameterResolver(param, commandClass.Name);
        }

        return new CommandInfo(commandClass, MethodCaller.From(method), new ParameterCollection(resolvers),
            hasMethodOptions ? methodOptions : null);
    }

    public async Task ExecuteAsync(ICommandContext context, IServiceProvider services, CancellationToken token)
    {
        var handler = GetHandler(services);
        var args = _parameters.CreateInstances(services, context, token);

        await _method.ExecuteAsync(handler, args, token);
    }
}