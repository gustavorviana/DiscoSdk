using DiscoSdk.Contexts.Interactions;
using System.Reflection;

namespace DiscoSdk.Commands;

internal record AutocompleteInfo(Type AutocompleteType, MethodInfo method)
{
    public Task ExecuteAsync(object instance, IAutocompleteContext context)
        => (Task)method.Invoke(instance, [context])!;
}
