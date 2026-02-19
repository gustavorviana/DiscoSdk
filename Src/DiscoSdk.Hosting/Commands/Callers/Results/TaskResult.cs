using System.Reflection;

namespace DiscoSdk.Hosting.Commands.Callers.Results;

internal class TaskResult(MethodInfo methodInfo) : MethodCaller(methodInfo)
{
    public override Task ExecuteAsync(object instance, object?[] parameters, CancellationToken token)
    {
        return (Task)Execute(instance, parameters)!;
    }
}
