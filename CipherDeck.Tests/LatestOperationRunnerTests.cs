using Xunit;

namespace CipherDeck.Tests;

public sealed class LatestOperationRunnerTests
{
    [Fact]
    public async Task NewOperationCancelsAndSupersedesPreviousResult()
    {
        using var runner = new LatestOperationRunner();
        using var firstStarted = new ManualResetEventSlim();

        var firstTask = runner.RunAsync(token =>
        {
            firstStarted.Set();
            token.WaitHandle.WaitOne();
            token.ThrowIfCancellationRequested();
            return 1;
        });
        Assert.True(firstStarted.Wait(TimeSpan.FromSeconds(5)));

        var secondResult = await runner.RunAsync(_ => 2);
        var firstResult = await firstTask;

        Assert.False(firstResult.IsCurrent);
        Assert.True(secondResult.IsCurrent);
        Assert.Equal(2, secondResult.Value);
    }

    [Fact]
    public async Task ExplicitCancellationDiscardsRunningResult()
    {
        using var runner = new LatestOperationRunner();
        using var operationStarted = new ManualResetEventSlim();

        var operationTask = runner.RunAsync(token =>
        {
            operationStarted.Set();
            token.WaitHandle.WaitOne();
            token.ThrowIfCancellationRequested();
            return "stale";
        });
        Assert.True(operationStarted.Wait(TimeSpan.FromSeconds(5)));

        runner.Cancel();
        var result = await operationTask;

        Assert.False(result.IsCurrent);
    }

    [Fact]
    public async Task OperationExceptionsAreNotHidden()
    {
        using var runner = new LatestOperationRunner();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runner.RunAsync<int>(_ => throw new InvalidOperationException("failure")));

        Assert.Equal("failure", exception.Message);
    }
}
