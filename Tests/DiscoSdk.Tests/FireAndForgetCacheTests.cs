using DiscoSdk.Hosting.Gateway.Events;

namespace DiscoSdk.Tests;

/// <summary>
/// Reflection-only tests for the cache that backs <see cref="FireAndForgetAttribute"/> detection.
/// Verifies method-level, class-level, inheritance, and absence semantics so callers can rely on
/// a single source of truth before the dispatcher branches.
/// </summary>
public class FireAndForgetCacheTests
{
    private class PlainClass { public void Plain() { } }

    [FireAndForget]
    private class MarkedClass { public void Method() { } }

    private class WithMarkedMethod
    {
        [FireAndForget]
        public void Marked() { }

        public void Unmarked() { }
    }

    private class InheritsMarkedClass : MarkedClass { }

    [Fact]
    public void IsFireAndForget_Type_Plain_ReturnsFalse()
        => Assert.False(FireAndForgetCache.IsFireAndForget(typeof(PlainClass)));

    [Fact]
    public void IsFireAndForget_Type_Marked_ReturnsTrue()
        => Assert.True(FireAndForgetCache.IsFireAndForget(typeof(MarkedClass)));

    [Fact]
    public void IsFireAndForget_Type_InheritsMarked_ReturnsTrue()
        => Assert.True(FireAndForgetCache.IsFireAndForget(typeof(InheritsMarkedClass)));

    [Fact]
    public void IsFireAndForget_Method_PlainOnPlainClass_ReturnsFalse()
    {
        var method = typeof(PlainClass).GetMethod(nameof(PlainClass.Plain))!;
        Assert.False(FireAndForgetCache.IsFireAndForget(method));
    }

    [Fact]
    public void IsFireAndForget_Method_MarkedMethod_ReturnsTrue()
    {
        var method = typeof(WithMarkedMethod).GetMethod(nameof(WithMarkedMethod.Marked))!;
        Assert.True(FireAndForgetCache.IsFireAndForget(method));
    }

    [Fact]
    public void IsFireAndForget_Method_UnmarkedOnMarkedClass_ReturnsTrue()
    {
        var method = typeof(MarkedClass).GetMethod(nameof(MarkedClass.Method))!;
        Assert.True(FireAndForgetCache.IsFireAndForget(method));
    }

    [Fact]
    public void IsFireAndForget_Method_UnmarkedOnPlainClass_ReturnsFalse()
    {
        var method = typeof(WithMarkedMethod).GetMethod(nameof(WithMarkedMethod.Unmarked))!;
        Assert.False(FireAndForgetCache.IsFireAndForget(method));
    }
}
