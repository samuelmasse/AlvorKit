namespace AlvorKit;

/// <summary>Prepares thread-owned arenas before timing and joins owners after the clock stops.</summary>
internal class ArchConcurrentFixture : IDisposable
{
    private readonly Thread[] threads;
    private readonly CountdownEvent ready;
    private readonly ManualResetEventSlim start = new(false);
    private readonly CountdownEvent done;
    private readonly ManualResetEventSlim release = new(false);
    private readonly System.Collections.Concurrent.ConcurrentQueue<Exception> failures = new();
    private long observed;
    internal long Observed => observed;

    internal ArchConcurrentFixture(int owners, Action<ArchConcurrentFixture, int> execute)
    {
        ready = new(owners);
        done = new(owners);
        threads = new Thread[owners];

        for (var index = 0; index < owners; index++)
        {
            var owner = index;
            threads[index] = new Thread(() => Execute(execute, owner))
            {
                IsBackground = true
            };
            threads[index].Start();
        }

        ready.Wait();
    }

    internal void ReadyAndWait()
    {
        ready.Signal();
        start.Wait();
    }

    internal void Complete(long value)
    {
        Interlocked.Add(ref observed, value);
        done.Signal();
        release.Wait();
    }

    internal void Run()
    {
        start.Set();
        done.Wait();
    }

    internal void ThrowIfFailed()
    {
        if (!failures.IsEmpty)
            throw new AggregateException(failures);
    }

    public void Dispose()
    {
        start.Set();
        release.Set();

        foreach (var thread in threads)
            thread.Join();
        ready.Dispose();
        start.Dispose();
        done.Dispose();
        release.Dispose();
    }

    private void Execute(Action<ArchConcurrentFixture, int> execute, int owner)
    {
        // A failed owner must still release both setup and completion waiters.
        try
        {
            execute(this, owner);
        }
        catch (Exception exception)
        {
            failures.Enqueue(exception);

            if (!start.IsSet)
                ready.Signal();

            if (!release.IsSet)
                done.Signal();
        }
    }
}
