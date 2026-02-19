using System.Reflection;

namespace DiscoSdk.Hosting.Commands.Callers.Results;

internal class VoidResult(MethodInfo method) : MethodCaller(method)
{
    public override Task ExecuteAsync(object instance, object?[] parameters, CancellationToken token)
    {
        Execute(instance, parameters);
        return Task.CompletedTask;
    }
}