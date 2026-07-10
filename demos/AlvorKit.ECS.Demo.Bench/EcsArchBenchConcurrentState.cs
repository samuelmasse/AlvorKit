namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Owns threads and gates for one alloc-partitioned concurrency sample.</summary>
internal sealed class EcsArchBenchConcurrentState(
    Thread[] threads,
    EntArena[] allocs,
    EntMut[][] ents,
    CountdownEvent ready,
    ManualResetEventSlim start,
    CountdownEvent done)
{
    internal Thread[] Threads { get; } = threads;
    internal EntArena[] Allocs { get; } = allocs;
    internal EntMut[][] Ents { get; } = ents;
    internal CountdownEvent Ready { get; } = ready;
    internal ManualResetEventSlim Start { get; } = start;
    internal CountdownEvent Done { get; } = done;

    internal void Join()
    {
        for (int i = 0; i < Threads.Length; i++)
            Threads[i].Join();
    }
}
