using DiscoSdk.Rest;

namespace DiscoSdk.Tests.Rest;

public class AuditLogReasonTests
{
    [Fact]
    public void Validate_PassesValidReason()
    {
        var result = AuditLogReason.Validate("Spamming in #general");

        Assert.Equal("Spamming in #general", result);
    }

    [Fact]
    public void Validate_PassesReasonExactlyAtCap()
    {
        var atCap = new string('x', AuditLogReason.MaxLength);

        var result = AuditLogReason.Validate(atCap);

        Assert.Equal(atCap, result);
    }

    [Fact]
    public void Validate_NullReason_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => AuditLogReason.Validate(null!));

        Assert.Contains("cannot be null", ex.Message);
    }

    [Fact]
    public void Validate_EmptyReason_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => AuditLogReason.Validate(string.Empty));

        Assert.Contains("cannot be null", ex.Message);
    }

    [Fact]
    public void Validate_WhitespaceReason_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => AuditLogReason.Validate("   \t\n"));

        Assert.Contains("cannot be null", ex.Message);
    }

    [Fact]
    public void Validate_OverLength_Throws()
    {
        var tooLong = new string('x', AuditLogReason.MaxLength + 1);

        var ex = Assert.Throws<ArgumentException>(() => AuditLogReason.Validate(tooLong));

        Assert.Contains("exceed", ex.Message);
        Assert.Contains(AuditLogReason.MaxLength.ToString(), ex.Message);
    }

    [Fact]
    public void HeaderName_MatchesDiscordContract()
    {
        Assert.Equal("X-Audit-Log-Reason", AuditLogReason.HeaderName);
    }

    [Fact]
    public void MaxLength_MatchesDiscordCap()
    {
        Assert.Equal(512, AuditLogReason.MaxLength);
    }
}
