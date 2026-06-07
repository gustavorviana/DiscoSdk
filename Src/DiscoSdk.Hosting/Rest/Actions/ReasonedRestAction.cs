using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Concrete <see cref="IReasonedRestAction"/> for bare mutating endpoints. The caller supplies an
/// executor that receives the reason at terminal time; the wrapper handles the
/// <c>WithReason</c> store-and-validate dance.
/// </summary>
internal sealed class ReasonedRestAction(Func<string?, CancellationToken, Task> executor)
    : RestAction, IReasonedRestAction
{
    private readonly Func<string?, CancellationToken, Task> _executor =
        executor ?? throw new ArgumentNullException(nameof(executor));
    private string? _reason;

    /// <inheritdoc />
    public IReasonedRestAction WithReason(string reason)
    {
        _reason = AuditLogReason.Validate(reason);
        return this;
    }

    /// <inheritdoc />
    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
        => _executor(_reason, cancellationToken);
}

/// <summary>
/// Concrete <see cref="IReasonedRestAction{T}"/> for bare mutating endpoints that return a payload.
/// </summary>
internal sealed class ReasonedRestAction<T>(Func<string?, CancellationToken, Task<T>> executor)
    : RestAction<T>, IReasonedRestAction<T>
{
    private readonly Func<string?, CancellationToken, Task<T>> _executor =
        executor ?? throw new ArgumentNullException(nameof(executor));
    private string? _reason;

    /// <inheritdoc />
    public IReasonedRestAction<T> WithReason(string reason)
    {
        _reason = AuditLogReason.Validate(reason);
        return this;
    }

    /// <inheritdoc />
    public override Task<T> ExecuteAsync(CancellationToken cancellationToken = default)
        => _executor(_reason, cancellationToken);
}
