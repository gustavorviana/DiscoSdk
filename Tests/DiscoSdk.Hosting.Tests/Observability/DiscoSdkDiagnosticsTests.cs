using DiscoSdk.Hosting.Observability;

namespace DiscoSdk.Hosting.Tests.Observability;

public class DiscoSdkDiagnosticsTests
{
    [Fact]
    public void MeterName_IsStableAndPublic()
    {
        Assert.Equal("DiscoSdk", DiscoSdkDiagnostics.MeterName);
    }

    [Fact]
    public void ActivitySourceName_IsStableAndPublic()
    {
        Assert.Equal("DiscoSdk", DiscoSdkDiagnostics.ActivitySourceName);
    }

    [Fact]
    public void ClassifyStatus_BucketsCommonCodes()
    {
        Assert.Equal("2xx", DiagnosticTags.ClassifyStatus(200));
        Assert.Equal("2xx", DiagnosticTags.ClassifyStatus(204));
        Assert.Equal("3xx", DiagnosticTags.ClassifyStatus(301));
        Assert.Equal("4xx", DiagnosticTags.ClassifyStatus(404));
        Assert.Equal("4xx", DiagnosticTags.ClassifyStatus(429));
        Assert.Equal("5xx", DiagnosticTags.ClassifyStatus(503));
    }

    [Fact]
    public void ClassifyStatus_OutOfRangeFallsBackToNumeric()
    {
        Assert.Equal("0", DiagnosticTags.ClassifyStatus(0));
        Assert.Equal("999", DiagnosticTags.ClassifyStatus(999));
    }
}
