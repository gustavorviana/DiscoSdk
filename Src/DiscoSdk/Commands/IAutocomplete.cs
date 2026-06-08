using DiscoSdk.Contexts.Interactions;

namespace DiscoSdk.Commands;

public interface IAutoComplete
{
    Task ExecuteAsync(IAutoCompleteContext context);
}