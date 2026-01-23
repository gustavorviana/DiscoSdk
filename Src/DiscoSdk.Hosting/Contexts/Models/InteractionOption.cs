using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Models;

internal class InteractionOption(object? value, IObjectConverter converter) : IInteractionOption
{
    public object? RawValue => value;

    public TValue? To<TValue>(TValue? @default = null) where TValue : class
    {
        if (RawValue == null)
            return @default;

        return (TValue?)converter.Convert(typeof(TValue), value);
    }

    public TValue? To<TValue>(TValue? @default = null) where TValue : struct
    {
        if (RawValue == null)
            return @default;

        return (TValue?)converter.Convert(typeof(TValue), value);
    }
}