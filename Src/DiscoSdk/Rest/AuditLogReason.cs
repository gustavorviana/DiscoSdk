namespace DiscoSdk.Rest;

/// <summary>
/// Centralises Discord's audit-log reason contract — the header name, the 512-character cap, and
/// the validation rule every <c>WithReason(string)</c> call funnels through. The value rides on
/// the <c>X-Audit-Log-Reason</c> request header (Discord URL-decodes it server-side and stores it
/// against the audit-log entry created by the action).
/// </summary>
/// <remarks>
/// Discord silently truncates reasons longer than <see cref="MaxLength"/>. Validating eagerly here
/// turns a silent truncation into a clear <see cref="ArgumentException"/> so moderators know their
/// log message will not be preserved verbatim. Empty / whitespace strings are rejected because
/// "blank reason" is indistinguishable from "no reason supplied" at the protocol level and only
/// adds noise to the audit log.
/// </remarks>
public static class AuditLogReason
{
    /// <summary>HTTP header Discord reads the reason from.</summary>
    public const string HeaderName = "X-Audit-Log-Reason";

    /// <summary>Hard maximum length Discord accepts on the header (in characters, not bytes).</summary>
    public const int MaxLength = 512;

    /// <summary>
    /// Validates a candidate reason against Discord's contract. Returns the trimmed reason on
    /// success, throws <see cref="ArgumentException"/> on null / whitespace / over-length input.
    /// </summary>
    /// <param name="reason">Caller-supplied reason text.</param>
    /// <returns>The same <paramref name="reason"/> value, for fluent assignment.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="reason"/> is null, empty, all whitespace, or exceeds <see cref="MaxLength"/>.
    /// </exception>
    public static string Validate(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Audit log reason cannot be null, empty, or whitespace.", nameof(reason));

        if (reason.Length > MaxLength)
            throw new ArgumentException(
                $"Audit log reason cannot exceed {MaxLength} characters; received {reason.Length}.",
                nameof(reason));

        return reason;
    }
}
