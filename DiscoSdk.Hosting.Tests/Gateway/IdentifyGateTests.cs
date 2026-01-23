
using DiscoSdk.Hosting.Gateway;

namespace DiscoSdk.Hosting.Tests.Gateway;

public class IdentifyGateTests
{
    [Fact]
    public void WaitAsync_WhenBelowMaxConcurrency_ReturnsCompletedTask()
    {
        // Arrange
        using var gate = new IdentifyGate();
        gate.SetMaxConcurrency(2);

        // Act
        var task1 = gate.WaitAsync();
        var task2 = gate.WaitAsync();

        // Assert
        Assert.True(task1.IsCompletedSuccessfully);
        Assert.True(task2.IsCompletedSuccessfully);
        Assert.Equal(2, gate.PendingReleaseCount);
    }

    [Fact]
    public void WaitAsync_WhenAtMaxConcurrency_ReturnsPendingTask()
    {
        // Arrange
        using var gate = new IdentifyGate();
        gate.SetMaxConcurrency(1);

        // Act
        var task1 = gate.WaitAsync();
        var task2 = gate.WaitAsync();

        // Assert
        Assert.True(task1.IsCompletedSuccessfully);
        Assert.False(task2.IsCompleted);
        Assert.Equal(2, gate.PendingReleaseCount);
    }

    [Fact]
    public async Task Release_WhenTasksWaiting_CompletesNextWaitingTaskAsync()
    {
        // Arrange
        using var gate = new IdentifyGate();
        gate.SetMaxConcurrency(1);

        var task1 = gate.WaitAsync();
        var task2 = gate.WaitAsync();
        var task3 = gate.WaitAsync();

        // Act
        gate.Release();

        await task2.WaitAsync(TimeSpan.FromMilliseconds(10));

        // Assert
        Assert.True(task1.IsCompletedSuccessfully);
        Assert.True(task2.IsCompletedSuccessfully);
        Assert.False(task3.IsCompleted);
        Assert.Equal(2, gate.PendingReleaseCount);
    }

    [Fact]
    public void Release_WhenNoTasksWaiting_DecrementsPendingCount()
    {
        // Arrange
        using var gate = new IdentifyGate();
        gate.SetMaxConcurrency(2);

        var task1 = gate.WaitAsync();
        var task2 = gate.WaitAsync();

        // Act
        gate.Release();

        // Assert
        Assert.True(task1.IsCompletedSuccessfully);
        Assert.True(task2.IsCompletedSuccessfully);
        Assert.Equal(1, gate.PendingReleaseCount);
    }

    [Fact]
    public async Task SetMaxConcurrency_WhenIncreased_ReleasesWaitingTasks()
    {
        // Arrange
        using var gate = new IdentifyGate();
        gate.SetMaxConcurrency(1);

        var task1 = gate.WaitAsync();
        var task2 = gate.WaitAsync();
        var task3 = gate.WaitAsync();

        // Act
        gate.SetMaxConcurrency(3);

        await Task.WhenAll(task1, task2, task3).WaitAsync(TimeSpan.FromMilliseconds(10));

        // Assert
        Assert.True(task1.IsCompletedSuccessfully);
        Assert.True(task2.IsCompletedSuccessfully);
        Assert.True(task3.IsCompletedSuccessfully);
        Assert.Equal(3, gate.PendingReleaseCount);
        Assert.Equal(3, gate.MaxConcurrency);
    }

    [Fact]
    public void SetMaxConcurrency_WhenDecreased_DoesNotCancelRunningTasks()
    {
        // Arrange
        using var gate = new IdentifyGate();
        gate.SetMaxConcurrency(3);

        var task1 = gate.WaitAsync();
        var task2 = gate.WaitAsync();
        var task3 = gate.WaitAsync();

        // Act
        gate.SetMaxConcurrency(1);

        // Assert
        Assert.True(task1.IsCompletedSuccessfully);
        Assert.True(task2.IsCompletedSuccessfully);
        Assert.True(task3.IsCompletedSuccessfully);
        Assert.Equal(1, gate.MaxConcurrency);
    }

    [Fact]
    public void SetMaxConcurrency_WithSameValue_DoesNothing()
    {
        // Arrange
        using var gate = new IdentifyGate();
        gate.SetMaxConcurrency(2);

        // Act
        gate.SetMaxConcurrency(2);

        // Assert
        Assert.Equal(2, gate.MaxConcurrency);
    }

    [Fact]
    public void SetMaxConcurrency_WithZeroOrNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using var gate = new IdentifyGate();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => gate.SetMaxConcurrency(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => gate.SetMaxConcurrency(-1));
    }

    [Fact]
    public async Task WaitAsync_WithCancellationToken_WhenCancelled_CancelsTask()
    {
        // Arrange
        using var gate = new IdentifyGate();
        gate.SetMaxConcurrency(1);
        using var cts = new CancellationTokenSource();

        var task1 = gate.WaitAsync();
        var task2 = gate.WaitAsync(cts.Token);

        // Act
        cts.Cancel();

        // Assert
        Assert.True(task1.IsCompletedSuccessfully);
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task2);
        Assert.Equal(1, gate.PendingReleaseCount);
    }

    [Fact]
    public async Task Dispose_CancelsAllWaitingTasks()
    {
        // Arrange
        var gate = new IdentifyGate();
        gate.SetMaxConcurrency(1);

        var task1 = gate.WaitAsync();
        var task2 = gate.WaitAsync();
        var task3 = gate.WaitAsync();

        // Act
        gate.Dispose();

        // Assert
        Assert.True(task1.IsCompletedSuccessfully);
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task2);
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task3);
        Assert.Equal(0, gate.PendingReleaseCount);
    }

    [Fact]
    public async Task Use_AfterDispose_ThrowsObjectDisposedExceptionAsync()
    {
        // Arrange
        var gate = new IdentifyGate();
        gate.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => gate.WaitAsync());
        Assert.Throws<ObjectDisposedException>(() => gate.Release());
        Assert.Throws<ObjectDisposedException>(() => gate.SetMaxConcurrency(2));
    }

    [Fact]
    public void PendingReleaseCount_ReflectsCurrentPendingCount()
    {
        // Arrange
        using var gate = new IdentifyGate();
        gate.SetMaxConcurrency(3);

        // Act
        var task1 = gate.WaitAsync();
        var task2 = gate.WaitAsync();
        var task3 = gate.WaitAsync();

        // Assert
        Assert.Equal(3, gate.PendingReleaseCount);

        // Act
        gate.Release();

        // Assert
        Assert.Equal(2, gate.PendingReleaseCount);
    }

    [Fact]
    public void MaxConcurrency_ReflectsCurrentMaxConcurrency()
    {
        // Arrange
        using var gate = new IdentifyGate();

        // Act & Assert
        Assert.Equal(1, gate.MaxConcurrency);

        gate.SetMaxConcurrency(5);
        Assert.Equal(5, gate.MaxConcurrency);

        gate.SetMaxConcurrency(10);
        Assert.Equal(10, gate.MaxConcurrency);
    }

    [Fact]
    public async Task MultipleReleases_ProcessesAllWaitingTasks()
    {
        // Arrange
        using var gate = new IdentifyGate();
        gate.SetMaxConcurrency(1);

        var task1 = gate.WaitAsync();
        var task2 = gate.WaitAsync();
        var task3 = gate.WaitAsync();
        var task4 = gate.WaitAsync();

        // Act
        gate.Release();
        await Task.Delay(10);
        gate.Release();
        await Task.Delay(10);
        gate.Release();
        await Task.Delay(10);
        gate.Release();

        // Assert
        Assert.True(task1.IsCompletedSuccessfully);
        Assert.True(task2.IsCompletedSuccessfully);
        Assert.True(task3.IsCompletedSuccessfully);
        Assert.True(task4.IsCompletedSuccessfully);
        Assert.Equal(0, gate.PendingReleaseCount);
    }

    [Fact]
    public void Release_WhenPendingCountIsZero_DoesNotThrow()
    {
        // Arrange
        using var gate = new IdentifyGate();
        gate.SetMaxConcurrency(1);

        var task1 = gate.WaitAsync();
        gate.Release();

        // Act & Assert
        gate.Release();
        Assert.Equal(0, gate.PendingReleaseCount);
    }
}