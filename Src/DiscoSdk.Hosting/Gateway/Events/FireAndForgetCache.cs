using DiscoSdk.Events;
using System.Collections.Concurrent;
using System.Reflection;

namespace DiscoSdk.Hosting.Gateway.Events;

internal static class FireAndForgetCache
{
    private static readonly ConcurrentDictionary<Type, FireAndForgetAttribute?> _typeCache = new();
    private static readonly ConcurrentDictionary<MethodInfo, FireAndForgetAttribute?> _methodCache = new();
    private static readonly ConcurrentDictionary<(Type Handler, Type Interface), FireAndForgetAttribute?> _handlerInterfaceCache = new();

    public static FireAndForgetAttribute? GetAttribute(Type type)
        => _typeCache.GetOrAdd(type, static t => t.GetCustomAttribute<FireAndForgetAttribute>(inherit: true));

    public static FireAndForgetAttribute? GetAttribute(MethodInfo method)
        => _methodCache.GetOrAdd(method, static m =>
            m.GetCustomAttribute<FireAndForgetAttribute>(inherit: true)
            ?? (m.DeclaringType is not null ? GetAttribute(m.DeclaringType) : null));

    /// <summary>
    /// Resolves the effective <see cref="FireAndForgetAttribute"/> for an event handler dispatched
    /// against a specific <see cref="IDiscordEventHandler{T}"/> interface. Order: class-level →
    /// method-level on the HandleAsync that implements the interface. Lets users place the
    /// attribute on either the handler class or the specific HandleAsync method.
    /// </summary>
    public static FireAndForgetAttribute? GetHandlerAttribute(Type handlerClass, Type handlerInterface)
        => _handlerInterfaceCache.GetOrAdd((handlerClass, handlerInterface), static key =>
        {
            // Class-level wins when present.
            var classAttr = GetAttribute(key.Handler);
            if (classAttr is not null) return classAttr;

            // Walk the interface map for the specific IDiscordEventHandler<T> contract so an
            // attribute on one HandleAsync only counts for that handler interface, not siblings.
            if (!key.Interface.IsAssignableFrom(key.Handler))
                return null;

            InterfaceMapping map;
            try { map = key.Handler.GetInterfaceMap(key.Interface); }
            catch (ArgumentException) { return null; }

            foreach (var method in map.TargetMethods)
            {
                var attr = method.GetCustomAttribute<FireAndForgetAttribute>(inherit: true);
                if (attr is not null) return attr;
            }
            return null;
        });

    public static bool IsFireAndForget(Type type) => GetAttribute(type) is not null;

    public static bool IsFireAndForget(MethodInfo method) => GetAttribute(method) is not null;
}
