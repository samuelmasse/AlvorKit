namespace AlvorKit;

internal static class ArchConcurrencyMeasurements
{
    internal static BenchResult Get<A>(int count, int owners)
    {
        using var fixture = new ArchConcurrentFixture(owners, (state, owner) => ArchConcurrentGet.Owner<A>(state, owner, count));
        var operations = count * owners;
        var timer = BenchTimer.Start();
        ArchConcurrentGet.Run(fixture);
        var result = timer.Stop(operations, "operation");
        fixture.ThrowIfFailed();
        GC.KeepAlive(fixture.Observed);
        // The timer's current-thread counter cannot describe work on owner threads.
        return result with
        {
            WorkloadAllocatedBytes = null
        };
    }

    internal static BenchResult Set<A>(int count, int owners)
    {
        using var fixture = new ArchConcurrentFixture(owners, (state, owner) => ArchConcurrentSet.Owner<A>(state, owner, count));
        var operations = count * owners;
        var timer = BenchTimer.Start();
        ArchConcurrentSet.Run(fixture);
        var result = timer.Stop(operations, "operation");
        fixture.ThrowIfFailed();
        GC.KeepAlive(fixture.Observed);
        // The timer's current-thread counter cannot describe work on owner threads.
        return result with
        {
            WorkloadAllocatedBytes = null
        };
    }

    internal static BenchResult Resolve<A>(int count, int owners)
    {
        using var fixture = new ArchConcurrentFixture(owners, (state, owner) => ArchConcurrentResolve.Owner<A>(state, owner, count));
        var operations = count * owners;
        var timer = BenchTimer.Start();
        ArchConcurrentResolve.Run(fixture);
        var result = timer.Stop(operations, "move");
        fixture.ThrowIfFailed();
        GC.KeepAlive(fixture.Observed);
        // The timer's current-thread counter cannot describe work on owner threads.
        return result with
        {
            WorkloadAllocatedBytes = null
        };
    }
}
