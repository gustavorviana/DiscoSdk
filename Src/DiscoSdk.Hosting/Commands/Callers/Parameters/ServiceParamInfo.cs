using DiscoSdk.Contexts;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands.Callers.Parameters;

internal class ServiceParamInfo(ParameterInfo parameter) : ParamInfo(parameter)
{
    public override object? CreateInstance(IServiceProvider provider, IContext context)
        => provider.GetRequiredService(Parameter.ParameterType);
}