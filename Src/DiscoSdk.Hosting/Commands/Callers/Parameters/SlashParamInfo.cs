using DiscoSdk.Commands;
using DiscoSdk.Contexts;
using DiscoSdk.Models.Enums;
using DiscoSdk.Utils;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands.Callers.Parameters;

internal abstract class SlashParamInfo : ParamInfo
{
    public SlashOptionAttribute? Option { get; }
    public AutocompleteInfo? Autocomplete { get; }
    public string Name => Option?.Name ?? Parameter.Name!;
    public abstract SlashCommandOptionType? Type { get; }
    public virtual bool Required => Option?.Required ?? false;

    public virtual int? MaxLength => Option?.GetMaxLength();
    public virtual object? MinValue => Option?.MinValue;
    public virtual object? MaxValue => Option?.MaxValue;

    public SlashParamInfo(ParameterInfo parameter, string commandName) : base(parameter)
    {
        Option = parameter.GetCustomAttribute<SlashOptionAttribute>();
        Autocomplete = AutocompleteInfo.GetOfOption(Option, commandName, Name);
    }

    public ChoiceAttribute[] GetChoices() => SlashOptionTypeUtils.GetChoices(Parameter);

    public static ParamInfo CreateParameterResolver(ParameterInfo param, string commandName)
    {
        if (param.GetCustomAttribute<FromServicesAttribute>() != null)
            return new ServiceParamInfo(param);

        if (typeof(IContext).IsAssignableFrom(param.ParameterType))
            return new ContextParameterInfo(param);

        return new ParamFactoryInfo(param, commandName);
    }
}