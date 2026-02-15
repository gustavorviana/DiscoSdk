using DiscoSdk.Contexts.Interactions;

namespace DiscoSdk.Commands;

public interface IAutocompleteHandler
{
    Task CompleteAsync(IAutocompleteContext context);
}