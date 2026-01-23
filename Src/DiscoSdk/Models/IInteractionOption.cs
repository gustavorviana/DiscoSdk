namespace DiscoSdk.Models;

public interface IInteractionOption
{
    object? RawValue { get; }

    TValue? To<TValue>(TValue? @default = default) where TValue : class;
    TValue? To<TValue>(TValue? @default = default) where TValue : struct;
}