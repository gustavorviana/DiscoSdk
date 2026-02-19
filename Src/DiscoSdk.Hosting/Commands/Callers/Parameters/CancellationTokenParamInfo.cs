using DiscoSdk.Contexts;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands.Callers.Parameters;

internal class CancellationTokenParamInfo(ParameterInfo parameter) : ParamInfo(parameter)
{
    public override object? CreateInstance(IServiceProvider provider, IContext context)
    {
        return default(CancellationToken);
    }
}
