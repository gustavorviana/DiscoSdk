using DiscoSdk.Contexts;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands.Callers.Parameters;

internal abstract class ParamInfo(ParameterInfo parameter)
{
    public ParameterInfo Parameter => parameter;

    public abstract object? CreateInstance(IServiceProvider services, IContext context);

    public static ParamInfo Create(ParameterInfo param)
    {
        if (typeof(IContext).IsAssignableFrom(param.ParameterType))
            return new ContextParameterInfo(param);

        return new ServiceParamInfo(param);
    }
}