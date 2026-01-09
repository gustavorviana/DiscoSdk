using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

public abstract class RestAction<T> : IRestAction<T>
{
    public abstract Task<T> ExecuteAsync(CancellationToken cancellationToken = default);

    public static RestAction<T> FromFunc(Func<T> func)
    {
        return new RestActionImpl<T>(_ => Task.FromResult(func()));
    }

    public static RestAction<T> Create(Func<CancellationToken, Task<T>> func)
    {
        return new RestActionImpl<T>(func);
    }

    Task IRestAction.ExecuteAsync(CancellationToken cancellationToken)
    {
        return ExecuteAsync(cancellationToken);
    }
}
public class RestAction(Func<CancellationToken, Task> action) : IRestAction
{
    public RestAction(Action func) : this(_ => Task.FromResult(func))
    {

    }

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return action(cancellationToken);
    }
}

file class RestActionImpl<T>(Func<CancellationToken, Task<T>> action) : RestAction<T>
{
    public override Task<T> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return action(cancellationToken);
    }
}