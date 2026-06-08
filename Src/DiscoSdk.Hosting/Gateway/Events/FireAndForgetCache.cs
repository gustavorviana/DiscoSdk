using System.Collections.Concurrent;
using System.Reflection;

namespace DiscoSdk.Hosting.Gateway.Events;

internal static class FireAndForgetCache
{
    private static readonly ConcurrentDictionary<Type, bool> _typeCache = new();
    private static readonly ConcurrentDictionary<MethodInfo, bool> _methodCache = new();

    public static bool IsFireAndForget(Type type)
        => _typeCache.GetOrAdd(type, static t =>
            t.GetCustomAttribute<FireAndForgetAttribute>(inherit: true) is not null);

    public static bool IsFireAndForget(MethodInfo method)
        => _methodCache.GetOrAdd(method, static m =>
            m.GetCustomAttribute<FireAndForgetAttribute>(inherit: true) is not null
            || (m.DeclaringType is not null && IsFireAndForget(m.DeclaringType)));
}
