namespace AlvorKit;

/// <summary>Tests notification coalescing and explicit failure without running MSBuild.</summary>
[TestClass]
public class SolutionWatchQueueTest
{
    /// <summary>Duplicate events coalesce, and events arriving during work remain queued for the next evaluation.</summary>
    [TestMethod]
    public async Task KeepsNewInvalidationsAfterDrainingBatch()
    {
        var queue = new SolutionWatchQueue();
        var first = new SolutionWatchRequest("First", false);
        var second = new SolutionWatchRequest("Second", true);

        for (var index = 0; index < 100; index++)
            queue.Add(first);

        queue.Add(second);
        var batch = await queue.ReadAsync(CancellationToken.None);
        CollectionAssert.AreEquivalent(new[] { first, second }, batch.ToArray());
        queue.Add(first);
        batch = await queue.ReadAsync(CancellationToken.None);
        CollectionAssert.AreEqual(new[] { first }, batch.ToArray());
        using var cancellation = new CancellationTokenSource();
        var idle = queue.ReadAsync(cancellation.Token);
        Assert.IsFalse(idle.IsCompleted);
        cancellation.Cancel();
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () => await idle);
    }

    /// <summary>Lost OS notifications terminate the session rather than silently scheduling a fallback scan.</summary>
    [TestMethod]
    public async Task ReportsNotificationOverflowAsFailure()
    {
        var queue = new SolutionWatchQueue();
        var failure = new InternalBufferOverflowException("lost events");
        queue.Fail(failure);
        var error = await Assert.ThrowsExactlyAsync<InternalBufferOverflowException>(
            async () => await queue.ReadAsync(CancellationToken.None));
        Assert.AreSame(failure, error);
    }
}
