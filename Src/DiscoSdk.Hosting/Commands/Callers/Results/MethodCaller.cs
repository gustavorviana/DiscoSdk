using DiscoSdk.Rest.Actions;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands.Callers.Results;

internal abstract class MethodCaller(MethodInfo method)
{
    public MethodInfo Method => method;

    public abstract Task ExecuteAsync(object instance, object?[] parameters, CancellationToken token);

    protected object? Execute(object instance, object?[] parameters)
    {
        return Method.Invoke(instance, parameters);
    }

    public static MethodCaller From(MethodInfo method)
    {
        if (method.ReturnType == typeof(Task))
            return new TaskResult(method);

        if (method.ReturnType == typeof(IRestAction))
            return new RestResult(method);

        return new VoidResult(method);
    }
}