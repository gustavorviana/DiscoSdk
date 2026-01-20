namespace DiscoSdk.Contexts.Interactions;

public interface IWithOptionCollection<TOption>
{
    IReadOnlyCollection<TOption> Options { get; }

    TOption? GetOption(string name);

    TValue? GetOption<TValue>(string name, TValue? @default = default) where TValue : class;
    TValue? GetOption<TValue>(string name, TValue? @default = default) where TValue : struct;
}