namespace DiscoSdk.Commands;

/// <summary>
/// Provides a value for a slash command parameter from the current command context (e.g. options, user, channel).
/// Register implementations in DI to resolve parameters that are not marked with <see cref="FromServicesAttribute"/>.
/// </summary>
/// <typeparam name="T">The type of the parameter value.</typeparam>
public interface IParamProvider<T> : IParamProvider
{
    /// <summary>
    /// Gets the value for the parameter from the given command context.
    /// </summary>
    Task<T?> GetValueAsync();
}

public interface IParamProvider;