using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands;

internal class CommandInfo
{
    public IReadOnlyDictionary<string, AutocompleteInfo> Autocompletes { get; }
    public SlashCommandAttribute Info { get; }
    public Type Type { get; }

    private CommandInfo(SlashCommandAttribute attribute, Type type)
    {
        Info = attribute;
        Type = type;

        Autocompletes = GetAutocompletes(type);
    }

    public SlashCommandBuilder GetCommandBuilder()
    {
        var builder = new SlashCommandBuilder();
        var choices = GetChoices();

        builder.WithName(Info.Name);
        builder.WithDescription(Info.Description);
        builder.WithType(Models.Enums.ApplicationCommandType.ChatInput);

        foreach (var option in Type.GetCustomAttributes<OptionAttribute>())
        {
            builder.AddOption(new Models.Commands.ApplicationCommandOption
            {
                Autocomplete = Autocompletes.ContainsKey(option.Name),
                Name = option.Name,
                ChannelTypes = option.ChannelTypes,
                Choices = choices.TryGetValue(option.Name, out var choice) ? [.. choice.Select(x => x.ToCommandChoice())] : [],
                Description = option.Description,
                MinLength = option.GetMinLength(),
                MaxLength = option.GetMaxLength(),
                MinValue = option.MinValue,
                MaxValue = option.MaxValue,
                Required = option.Required,
                Type = option.Type,
            });
        }

        return builder;
    }

    private IReadOnlyDictionary<string, List<ChoiceAttribute>> GetChoices()
    {
        var cmdChoices = new Dictionary<string, List<ChoiceAttribute>>();

        var enumChoices = Type.GetCustomAttribute<EnumChoicesAttribute>();
        if (enumChoices != null)
        {
            if (!cmdChoices.TryGetValue(enumChoices.OptionName, out var choices))
            {
                choices = [];
                cmdChoices.Add(enumChoices.OptionName, choices);
            }

            choices.AddRange(enumChoices.GetChoices());
        }

        foreach (var choice in Type.GetCustomAttributes<ChoiceAttribute>())
        {
            if (!cmdChoices.TryGetValue(choice.OptionName, out var choices))
            {
                choices = [];
                cmdChoices.Add(choice.OptionName, choices);
            }

            choices.Add(choice);
        }

        return cmdChoices;
    }

    public Task ExecuteCommandAsync(IServiceProvider service, ICommandContext context)
    {
        var handler = service.GetRequiredService(Type) as SlashCommandHandler;
        return handler!.ExecuteAsync(context);
    }

    public async Task ExecuteAutocompleteAsync(IServiceProvider service, IAutocompleteContext context)
    {
        if (!Autocompletes.TryGetValue(context.FocusedOption.Name, out var autocompleteInfo))
            throw new InvalidOperationException(
                $"No autocomplete handler registered for option '{context.FocusedOption.Name}'.");

        var handler = service.GetRequiredService(autocompleteInfo.AutocompleteType);
        await autocompleteInfo.ExecuteAsync(handler, context);
    }

    private IReadOnlyDictionary<string, AutocompleteInfo> GetAutocompletes(Type commandType)
    {
        var items = new Dictionary<string, AutocompleteInfo>(StringComparer.OrdinalIgnoreCase);
        var contextType = typeof(IAutocompleteContext);
        var autocompleteHandlerType = typeof(IAutocompleteHandler);

        foreach (var autocompleteOption in commandType.GetCustomAttributes<OptionAttribute>())
        {
            if (autocompleteOption.AutocompleteType == null)
                continue;

            var autoCompleteType = autocompleteOption.AutocompleteType;

            if (!autocompleteHandlerType.IsAssignableFrom(autocompleteOption.AutocompleteType))
                throw new InvalidOperationException(
                $"Type '{autocompleteOption.AutocompleteType.FullName}' must implement or inherit '{autocompleteHandlerType.FullName}'.");

            var name = autocompleteOption.Name;

            if (items.ContainsKey(name))
                throw new InvalidOperationException($"Command \"{name}\" already exists.");

            items[name] = new AutocompleteInfo(autoCompleteType, ReflectionUtils.FindInterfaceMethod(autoCompleteType, autocompleteHandlerType, nameof(IAutocompleteHandler.CompleteAsync))!);
        }

        foreach (var method in commandType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var attribute = method.GetCustomAttribute<AutocompleteHandlerAttribute>();
            if (attribute == null)
                continue;

            var methodParams = method.GetParameters();
            if (methodParams.Length != 1 || methodParams.First().ParameterType != contextType)
                continue;

            if (items.ContainsKey(attribute.OptionName))
                throw new InvalidOperationException($"Command \"{attribute.OptionName}\" already exists.");

            items[attribute.OptionName] = new AutocompleteInfo(commandType, method);
        }

        return items;
    }

    internal static IEnumerable<CommandInfo> GetAll(Assembly[] assemblies)
    {
        foreach (var type in FindSlashCommandHandlers(assemblies))
        {
            var commandAttribute = type.GetCustomAttribute<SlashCommandAttribute>();
            if (commandAttribute != null)
                yield return new CommandInfo(commandAttribute, type);
        }
    }

    private static IEnumerable<Type> FindSlashCommandHandlers(Assembly[] assemblies)
    {
        return assemblies
            .SelectMany(GetLoadableTypes)
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                typeof(SlashCommandHandler).IsAssignableFrom(t));
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }
}
