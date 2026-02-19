using DiscoSdk.Rest.Actions;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands.Callers.Results;

internal class RestResult(MethodInfo method) : MethodCaller(method)
{
    public override Task ExecuteAsync(object instance, object?[] parameters, CancellationToken token)
    {
        var result = Execute(instance, parameters);
        return ((IRestAction)result!).ExecuteAsync(token);
    }
}