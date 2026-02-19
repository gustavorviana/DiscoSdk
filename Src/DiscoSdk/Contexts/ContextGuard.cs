namespace DiscoSdk.Contexts;

internal static class ContextGuard
{
    public static TContext Require<TContext>(ISdkContextProvider provider)
        where TContext : class, IContext
    {
        ArgumentNullException.ThrowIfNull(provider);

        var ctx = provider.GetContext();
        if (ctx is TContext typed)
            return typed;

        throw CreateWrongContextException(typeof(TContext), ctx);
    }

    private static InvalidOperationException CreateWrongContextException(Type expectedContextType, IContext? actual)
    {
        var actualName = actual?.GetType().Name ?? "null";

        return new InvalidOperationException(
            $"The current interaction context does not provide '{expectedContextType.Name}'. " +
            $"Expected {expectedContextType.Name}, but got {actualName}.");
    }
}