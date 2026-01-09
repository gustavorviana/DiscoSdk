using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

public abstract class RestAction<T> : IRestAction<T>
{
    public abstract Task<T> ExecuteAsync(CancellationToken cancellationToken = default);

    public static RestAction<T> FromResult(T value)
    {
        return new RestActionImpl<T>(_ => Task.FromResult(value));
    }

    public static RestAction<T> Create(Func<CancellationToken, Task<T>> func)
    {
        return new RestActionImpl<T>(func);
    }

    void IRestAction.Execute()
    {
        ExecuteAsync().GetAwaiter().GetResult();
    }

    Task IRestAction.ExecuteAsync(CancellationToken cancellationToken)
    {
        return ExecuteAsync(cancellationToken);
    }

    public T Execute()
    {
        return ExecuteAsync().GetAwaiter().GetResult();
    }
}

public abstract class RestAction : IRestAction
{
    public static RestAction Create(Func<CancellationToken, Task> func)
    {
        return new RestActionImpl(cancellationToken => Task.Run(() => func(cancellationToken)));
    }

    public void Execute()
    {
        ExecuteAsync().GetAwaiter().GetResult();
    }

    public abstract Task ExecuteAsync(CancellationToken cancellationToken = default);
}

file class RestActionImpl(Func<CancellationToken, Task> action) : RestAction
{
    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
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