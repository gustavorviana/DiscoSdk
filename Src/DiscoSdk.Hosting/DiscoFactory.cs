using Microsoft.Extensions.DependencyInjection;

namespace DiscoSdk.Hosting;

/// <summary>
/// Accumulates module / event-handler registrations and materializes them into a flat list via
/// <see cref="Build"/>. One-shot: after <see cref="Build"/> runs the factory is cleared and
/// locked — further <c>Add*</c> or <c>Build</c> calls throw. Mirrors the lock-after-build pattern
/// of <see cref="Commands.CommandRegistryBuilder"/>.
/// </summary>
public sealed class DiscoFactory<TBase> where TBase : class
{
    private readonly HashSet<Entry> _entries = [];
    private bool _built;

    public DiscoFactory<TBase> Add(Type type)
    {
        EnsureNotBuilt();
        ArgumentNullException.ThrowIfNull(type);

        var entry = Entry.FromType(type);
        if (!_entries.Add(entry))
            throw new InvalidOperationException($"Type '{type.FullName}' is already registered for '{typeof(TBase).FullName}'.");

        return this;
    }

    public DiscoFactory<TBase> Add<T>() where T : class, TBase
        => Add(typeof(T));

    public DiscoFactory<TBase> AddInstance(TBase instance)
    {
        EnsureNotBuilt();
        ArgumentNullException.ThrowIfNull(instance);

        var entry = Entry.FromInstance(instance);
        if (!_entries.Add(entry))
            throw new InvalidOperationException($"Type '{entry.Type.FullName}' is already registered for '{typeof(TBase).FullName}'.");

        return this;
    }

    public IReadOnlyCollection<Type> GetTypes()
    {
        if (_entries.Count == 0)
            return [];

        var list = new List<Type>(_entries.Count);
        foreach (var e in _entries)
            list.Add(e.Type);

        return list.AsReadOnly();
    }

    /// <summary>
    /// Materializes every entry into a flat list and locks the factory. Result instances are
    /// owned by the caller; this factory drops all internal state and refuses further
    /// <c>Add*</c> / <c>Build</c> calls (even if Build throws).
    /// </summary>
    public IReadOnlyList<TBase> Build(IServiceProvider services)
    {
        EnsureNotBuilt();
        ArgumentNullException.ThrowIfNull(services);

        try
        {
            if (_entries.Count == 0)
                return [];

            var result = new List<TBase>(_entries.Count);

            foreach (var entry in _entries)
            {
                if (entry.Instance is not null)
                {
                    result.Add(entry.Instance);
                    continue;
                }

                if (services.GetService(entry.Type) is TBase diInstance)
                {
                    result.Add(diInstance);
                    continue;
                }

                var created = ActivatorUtilities.CreateInstance(services, entry.Type);
                if (created is not TBase typed)
                    throw new InvalidOperationException($"Type '{entry.Type.FullName}' did not produce '{typeof(TBase).FullName}'.");

                result.Add(typed);
            }

            return result;
        }
        finally
        {
            // Contract is one-shot: lock and drop entries even if materialization threw
            // halfway through. A partial build isn't salvageable.
            _entries.Clear();
            _built = true;
        }
    }

    private void EnsureNotBuilt()
    {
        if (_built)
            throw new InvalidOperationException(
                $"DiscoFactory<{typeof(TBase).Name}> has already been built and is locked — no further Add or Build calls allowed.");
    }

    private readonly struct Entry : IEquatable<Entry>
    {
        public Type Type { get; }
        public TBase? Instance { get; }

        private Entry(Type type, TBase? instance)
        {
            Type = type;
            Instance = instance;
        }

        public static Entry FromType(Type type)
        {
            ValidateType(type);
            return new Entry(type, null);
        }

        public static Entry FromInstance(TBase instance)
        {
            var type = instance.GetType();
            ValidateType(type);
            return new Entry(type, instance);
        }

        public bool Equals(Entry other) => Type == other.Type;

        public override bool Equals(object? obj) => obj is Entry other && Equals(other);

        public override int GetHashCode() => Type.GetHashCode();

        private static void ValidateType(Type type)
        {
            if (!typeof(TBase).IsAssignableFrom(type))
                throw new InvalidOperationException($"Type '{type.FullName}' must implement/derive from '{typeof(TBase).FullName}'.");

            if (type.IsAbstract || type.IsInterface)
                throw new InvalidOperationException($"Type '{type.FullName}' must be a concrete class.");

            if (type.ContainsGenericParameters)
                throw new InvalidOperationException($"Type '{type.FullName}' must be a closed constructed type (no open generics).");
        }
    }
}
