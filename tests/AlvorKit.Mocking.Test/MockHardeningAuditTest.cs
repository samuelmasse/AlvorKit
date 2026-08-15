namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public sealed class MockHardeningAuditTest
{
    /// <summary>A loose collection-interface return publishes one stable non-throwing default.</summary>
    [TestMethod]
    public void LooseFallback_CollectionInterfacePublishesStableDefault()
    {
        var target = Mock.CreateLoose<IHardeningDefaultTarget>();

        System.Collections.ICollection first = target.Collection;

        Assert.IsNotNull(first);
        Assert.AreSame(first, target.Collection);
    }

    /// <summary>A loose default never runs a caller-owned collection constructor under the mock-state lock.</summary>
    [TestMethod]
    public void LooseFallback_UserConstructorRunsOutsideMockLock()
    {
        var target = Mock.CreateLoose<IHardeningDefaultTarget>();
        Mocked mocked = Mock.GetMocked(target)!;
        var observedLock = false;
        HardeningCollection.OnConstruct =
            () => observedLock = Monitor.IsEntered(mocked);

        try
        {
            _ = target.ConcreteCollection;
        }
        finally
        {
            HardeningCollection.OnConstruct = null;
        }

        Assert.IsFalse(
            observedLock,
            "Loose default construction ran caller code while holding Mocked.");
    }

    /// <summary>Concurrent first use publishes one loose default and constructs it exactly once.</summary>
    [TestMethod]
    public void LooseFallback_ConcurrentFirstUsePublishesOneConstructedDefault()
    {
        const int callerCount = 16;
        var target = Mock.CreateLoose<IHardeningDefaultTarget>();
        var results = new HardeningCollection[callerCount];
        var callers = new Task[callerCount];
        using var start = new Barrier(callerCount + 1);
        HardeningCollection.ResetConstructionCount();

        for (var index = 0; index < callerCount; index++)
        {
            int capture = index;
            callers[index] = Task.Factory.StartNew(
                () =>
                {
                    Assert.IsTrue(
                        start.SignalAndWait(TimeSpan.FromSeconds(10)),
                        "Concurrent loose-default callers failed to rendezvous.");
                    results[capture] = target.ConcreteCollection;
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        Assert.IsTrue(
            start.SignalAndWait(TimeSpan.FromSeconds(10)),
            "The loose-default test failed to release concurrent callers.");
        Task.WaitAll(callers);

        Assert.AreEqual(1, HardeningCollection.ConstructionCount);
        for (var index = 1; index < results.Length; index++)
            Assert.AreSame(results[0], results[index]);
    }
}

internal interface IHardeningDefaultTarget
{
    System.Collections.ICollection Collection { get; }

    HardeningCollection ConcreteCollection { get; }
}

internal sealed class HardeningCollection : List<object>
{
    private static int constructionCount;

    internal static Action? OnConstruct;

    internal static int ConstructionCount =>
        Volatile.Read(ref constructionCount);

    public HardeningCollection()
    {
        Interlocked.Increment(ref constructionCount);
        OnConstruct?.Invoke();
    }

    internal static void ResetConstructionCount() =>
        Volatile.Write(ref constructionCount, 0);
}
