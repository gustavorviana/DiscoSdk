using DiscoSdk.Contexts;
using System.Collections;

namespace DiscoSdk.Hosting.Commands.Callers.Parameters;

internal class ParameterCollection : IReadOnlyList<ParamInfo>
{
    private readonly IReadOnlyList<ParamInfo> _parameters;

    // -1 = none
    private readonly int _cancellationTokenIndex;

    public int Count => _parameters.Count;

    public ParamInfo this[int index] => _parameters[index];

    public ParameterCollection(IReadOnlyList<ParamInfo> parameters)
    {
        _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

        int idx = -1;

        for (int i = 0; i < _parameters.Count; i++)
        {
            if (_parameters[i] is CancellationTokenParamInfo)
            {
                if (idx != -1)
                    throw new InvalidOperationException("Only one CancellationToken parameter is allowed per command.");

                idx = i;
            }
        }

        _cancellationTokenIndex = idx;
    }

    public IEnumerator<ParamInfo> GetEnumerator() => _parameters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public object?[] CreateInstances(IServiceProvider services, IContext context, CancellationToken token)
    {
        var instances = new object?[Count];

        for (int i = 0; i < Count; i++)
            if (_cancellationTokenIndex == i) instances[i] = token;
            else instances[i] = _parameters[i].CreateInstance(services, context);

        return instances;
    }
}
