using DiscoSdk.Commands;
using DiscoSdk.Contexts;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands.Callers.Parameters;

internal class ParamFactoryInfo : SlashParamInfo
{
    private readonly Type _type;

    public override SlashCommandOptionType? Type { get; }
    public override bool Required { get; }
    public override int? MaxLength { get; }
    public override object? MinValue { get; }
    public override object? MaxValue { get; }

    public ParamFactoryInfo(ParameterInfo parameter, string commandName) : base(parameter, commandName)
    {
        _type = typeof(IParamProvider<>).MakeGenericType(parameter.ParameterType);
        Type = Option?.Type;

        if (Option?.Type != null)
        {
            Required = Option.Required;
            MaxLength = Option.MaxLength;

            MinValue = Option.MinValue;
            MaxValue = Option.MaxValue;
            return;
        }

        Type = GetCommandType();
        Required = SlashOptionTypeUtils.IsRequired(_type);

        MaxLength = SlashOptionTypeUtils.GetMaxLength(_type);

        MinValue = SlashOptionTypeUtils.GetMinValue(_type);
        MaxValue = SlashOptionTypeUtils.GetMaxValue(_type);
    }

    public override object? CreateInstance(IServiceProvider provider, IContext context)
    {
        var factory = provider.GetRequiredService(_type);
        var getValue = _type.GetMethod(nameof(IParamProvider<object>.GetValueAsync), [typeof(ICommandContext)]);
        return getValue?.Invoke(factory, [context]);
    }

    private SlashCommandOptionType GetCommandType()
    {
        if (_type == typeof(Snowflake))
        {
            throw new InvalidOperationException(
                "Snowflake parameters are not supported directly. " +
                $"You must decorate the parameter with [{typeof(SlashOptionAttribute).FullName}] and explicitly define its type. " +
                "Use SlashCommandOptionType.User (6), Channel (7), Role (8), or Mentionable (9)."
            );
        }

        var underlining = Nullable.GetUnderlyingType(_type) ?? _type;
        if (underlining.IsEnum)
            return SlashCommandOptionType.String;

        var types = SlashOptionTypeUtils.GetCommandTypeAssociations();
        if (!types.TryGetValue(_type, out var slashType))
            return slashType;

        throw new NotSupportedException(
            $"Type '{_type.FullName}' is not supported as a slash command option. " +
            "If this is a Discord entity (User, Channel, Role, Mentionable), use Snowflake " +
            "and explicitly specify the SlashCommandOptionType via SlashOptionAttribute."
        );
    }
}