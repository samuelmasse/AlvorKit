namespace AlvorKit;

/// <summary>Owns the background collection and console lifetime for one application log.</summary>
public sealed class LogRuntime : IDisposable
{
    /// <summary>Serializes runtime lifecycle changes.</summary>
    private readonly Lock lifecycle = new();
    /// <summary>Accepts producer entries and publishes ordered segments.</summary>
    private readonly LogStream stream = new();
    /// <summary>Writes published segments to the configured destination.</summary>
    private readonly LogConsole console;
    /// <summary>Caches the segment-consumer callback without per-drain allocation.</summary>
    private readonly Action print;
    /// <summary>Wakes the background collector for flush and stop requests.</summary>
    private readonly AutoResetEvent wake = new(false);
    /// <summary>Signals completion of the current flush request.</summary>
    private readonly ManualResetEventSlim flushed = new(true);
    /// <summary>Runs collection while the runtime is started.</summary>
    private Thread? worker;
    /// <summary>Tracks a pending synchronous flush request.</summary>
    private bool flushRequested;
    /// <summary>Tracks a pending stop request.</summary>
    private bool stopRequested;
    /// <summary>Tracks whether owned resources have been released.</summary>
    private bool disposed;

    /// <summary>Creates a runtime that writes to the process console.</summary>
    public LogRuntime() : this(Console.Out) { }

    /// <summary>Creates a runtime that writes to <paramref name="output"/>.</summary>
    public LogRuntime(TextWriter output)
    {
        console = new(stream, output);
        print = console.Print;
        Log = new(stream);
    }

    /// <summary>Gets the producer API shared by the application.</summary>
    public Log Log { get; }

    /// <summary>Gets or sets whether console output uses ANSI level colors.</summary>
    public bool UseColor
    {
        get => console.UseColor;
        set => console.UseColor = value;
    }

    /// <summary>Starts background collection. Calling this while already running has no effect.</summary>
    public void Start()
    {
        lock (lifecycle)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (worker != null)
                return;

            stopRequested = false;
            flushRequested = false;
            worker = new(Work)
            {
                IsBackground = true,
                Name = "AlvorKit Log"
            };
            worker.Start();
        }
    }

    /// <summary>Synchronously publishes and writes every entry submitted before this call.</summary>
    public void Flush()
    {
        Thread? stoppingWorker = null;

        lock (lifecycle)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (worker == null)
            {
                Drain(true);
                return;
            }

            if (stopRequested)
            {
                stoppingWorker = worker;
            }
            else
            {
                flushed.Reset();
                flushRequested = true;
                wake.Set();
            }
        }

        if (stoppingWorker != null)
            stoppingWorker.Join();
        else
            flushed.Wait();
    }

    /// <summary>Drains pending entries and stops collection. Calling this while stopped has no effect.</summary>
    public void Stop()
    {
        Thread? current;
        lock (lifecycle)
        {
            current = worker;
            if (current == null)
                return;

            stopRequested = true;
            wake.Set();
        }

        current.Join();

        lock (lifecycle)
        {
            if (ReferenceEquals(worker, current))
                worker = null;
        }
    }

    /// <summary>Stops collection and releases runtime synchronization resources.</summary>
    public void Dispose()
    {
        lock (lifecycle)
        {
            if (disposed)
                return;
        }

        Stop();

        lock (lifecycle)
        {
            if (disposed)
                return;

            disposed = true;
            flushed.Dispose();
            wake.Dispose();
            stream.Dispose();
        }
    }

    /// <summary>Collects and writes entries until a stop is requested.</summary>
    private void Work()
    {
        while (true)
        {
            wake.WaitOne(3);

            bool shouldStop;
            bool shouldFlush;
            lock (lifecycle)
            {
                shouldStop = stopRequested;
                shouldFlush = flushRequested;
                flushRequested = false;
            }

            Drain(shouldStop || shouldFlush);
            if (shouldFlush)
                flushed.Set();
            if (shouldStop)
                return;
        }
    }

    /// <summary>Publishes eligible entries and writes every available segment.</summary>
    private void Drain(bool all)
    {
        stream.Collect(all ? 0 : 5, print);
        console.Print();
    }
}
