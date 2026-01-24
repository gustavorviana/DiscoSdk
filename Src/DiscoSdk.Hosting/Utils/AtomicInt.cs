namespace DiscoSdk.Hosting.Utils;

/// <summary>
/// Atomic integer wrapper.
/// 
/// IMPORTANT:
/// - Guarantees atomicity of single operations on the value.
/// - DOES NOT guarantee thread-safe logic for multi-step decisions.
/// - Backed by System.Threading.Interlocked.
/// </summary>
/// <remarks>
/// Creates a new atomic integer with an optional initial value.
/// </remarks>
internal sealed class AtomicInt(int initialValue = 0)
{
    private int _value = initialValue;

    /// <summary>
    /// Atomically reads the current value.
    /// Internally calls: Interlocked.CompareExchange(ref _value, 0, 0)
    /// </summary>
    public int Read()
    {
        return Interlocked.CompareExchange(ref _value, 0, 0);
    }

    /// <summary>
    /// Atomically increments the value by 1.
    /// Returns the new value.
    /// Internally calls: Interlocked.Increment(ref _value)
    /// </summary>
    public int Increment()
    {
        return Interlocked.Increment(ref _value);
    }

    /// <summary>
    /// Atomically decrements the value by 1.
    /// Returns the new value.
    /// Internally calls: Interlocked.Decrement(ref _value)
    /// </summary>
    public int Decrement()
    {
        return Interlocked.Decrement(ref _value);
    }

    /// <summary>
    /// Atomically adds <paramref name="delta"/> to the value.
    /// Returns the new value.
    /// Internally calls: Interlocked.Add(ref _value, delta)
    /// </summary>
    public int Add(int delta)
    {
        return Interlocked.Add(ref _value, delta);
    }

    /// <summary>
    /// Atomically replaces the current value with <paramref name="value"/>.
    /// Returns the previous value.
    /// Internally calls: Interlocked.Exchange(ref _value, value)
    /// </summary>
    public int Exchange(int value)
    {
        return Interlocked.Exchange(ref _value, value);
    }

    /// <summary>
    /// Atomically sets the value to <paramref name="newValue"/> 
    /// only if the current value equals <paramref name="expected"/>.
    /// Returns true if the exchange happened.
    /// Internally calls:
    /// Interlocked.CompareExchange(ref _value, newValue, expected)
    /// </summary>
    public bool CompareAndSet(int expected, int newValue)
    {
        return Interlocked.CompareExchange(ref _value, newValue, expected) == expected;
    }

    /// <summary>
    /// Atomically attempts to change a 0/1 flag from 0 to 1.
    /// Commonly used for "start once" or "already running" guards.
    /// Internally calls:
    /// Interlocked.CompareExchange(ref _value, 1, 0)
    /// </summary>
    public bool TrySetFlag()
    {
        return Interlocked.CompareExchange(ref _value, 1, 0) == 0;
    }

    /// <summary>
    /// Atomically clears a 0/1 flag (sets it to 0).
    /// Internally calls:
    /// Interlocked.Exchange(ref _value, 0)
    /// </summary>
    public void ClearFlag()
    {
        Interlocked.Exchange(ref _value, 0);
    }

    /// <summary>
    /// Returns the string representation of the current value.
    /// Uses an atomic read.
    /// </summary>
    public override string ToString()
    {
        return Read().ToString();
    }
}