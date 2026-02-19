using DiscoSdk.Contexts;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands.Callers.Parameters;

internal class ContextParameterInfo(ParameterInfo parameter) : ParamInfo(parameter)
{
    public override object? CreateInstance(IServiceProvider provider, IContext context)
    {
        if (!Parameter.ParameterType.IsAssignableFrom(context?.GetType()))
            return null;

        return context;
    }
}