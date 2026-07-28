namespace AlvorKit.Logging;

/// <summary>Collects per-thread buffers into globally ordered reusable segments.</summary>
internal sealed class LogStream : IDisposable
{
    /// <summary>Tracks every active producer thread.</summary>
    private readonly List<LogThread> logs;
    /// <summary>Provides each producer with its thread-owned buffers.</summary>
    private readonly ThreadLocal<LogThread> localLogThread;
    /// <summary>Reuses storage for entries collected during one pass.</summary>
    private readonly List<LogBufferEntry> aggregateEntries = [];
    /// <summary>Reuses sortable timestamp keys parallel to collected entries.</summary>
    private readonly List<DateTime> aggregateTimes = [];
    /// <summary>Tracks how far each source buffer may advance after commit.</summary>
    private readonly List<(LogThread, LogBuffer, int)> aggregateReads = [];
    /// <summary>Stores the bounded ring of published entry batches.</summary>
    private readonly LogSegment[] segments;
    /// <summary>Tracks the current writable segment in the ring.</summary>
    private long segmentIndex;
    /// <summary>Counts entries published into segments.</summary>
    private long logCount;

    /// <summary>Gets the reusable segment ring.</summary>
    public ReadOnlySpan<LogSegment> Segments => segments;
    /// <summary>Gets the current writable segment index.</summary>
    public long SegmentIndex => segmentIndex;
    /// <summary>Gets the number of entries published into segments.</summary>
    public long LogCount => logCount;

    /// <summary>Creates a stream with isolated buffers for each producer thread.</summary>
    public LogStream()
    {
        logs = [];
        localLogThread = new(CreateLogThreads);

        segments = new LogSegment[5];
        for (int i = 0; i < segments.Length; i++)
            segments[i] = new();
    }

    /// <summary>Adds one entry to the calling thread's producer buffer.</summary>
    public void Add(LogEntry entry, ReadOnlySpan<char> chars) => localLogThread.Value!.Add(entry, chars);

    /// <summary>Gets cleared, thread-owned storage for one formatted producer entry.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LogEntryText OpenText() => localLogThread.Value!.OpenText();

    /// <summary>Collects entries old enough for publication and advances their buffers.</summary>
    public void Collect(double ms, Action segmentClosed)
    {
        CollectEntries(ms);
        SortEntries();
        CommitEntries(segmentClosed);
        AdvanceBuffers();
        CleanupLogThreads();
    }

    /// <summary>Copies eligible producer entries into reusable aggregate storage.</summary>
    private void CollectEntries(double ms)
    {
        aggregateEntries.Clear();
        aggregateTimes.Clear();
        aggregateReads.Clear();

        var now = DateTime.UtcNow;

        lock (logs)
        {
            foreach (var log in CollectionsMarshal.AsSpan(logs))
            {
                foreach (var buffer in log.Buffers)
                {
                    var entries = buffer.Read();
                    if (entries.Length == 0)
                        continue;

                    int count = 0;
                    while (count < entries.Length &&
                        (ms <= 0 || (now - entries[count].Entry.Time).TotalMilliseconds >= ms))
                    {
                        aggregateEntries.Add(entries[count]);
                        aggregateTimes.Add(entries[count].Entry.Time);
                        count++;
                    }

                    aggregateReads.Add((log, buffer, count));
                }
            }
        }
    }

    /// <summary>Orders collected entries by their captured UTC timestamps.</summary>
    private void SortEntries()
    {
        var spanEntries = CollectionsMarshal.AsSpan(aggregateEntries);
        var spanTimes = CollectionsMarshal.AsSpan(aggregateTimes);
        spanTimes.Sort(spanEntries);
    }

    /// <summary>Copies ordered entries into segments, consuming full segments immediately.</summary>
    private void CommitEntries(Action segmentClosed)
    {
        foreach (var entry in aggregateEntries)
        {
            var segment = segments[segmentIndex % segments.Length];
            if (segment.Capacity == 0 || segment.CharCapacity < entry.Chars.Length)
            {
                segment.Close();
                segmentClosed();
                segment = segments[(segmentIndex + 1) % segments.Length];
                segment.Reset();
                segmentIndex++;
            }

            segment.Add(entry);
            logCount++;
        }
    }

    /// <summary>Marks committed producer entries consumed and wakes blocked producers.</summary>
    private void AdvanceBuffers()
    {
        foreach (var (log, buffer, count) in aggregateReads)
        {
            buffer.Advance(count);

            lock (log)
            {
                System.Threading.Monitor.Pulse(log);
            }
        }
    }

    /// <summary>Removes terminated producers after all of their entries are consumed.</summary>
    private void CleanupLogThreads()
    {
        lock (logs)
        {
            for (int i = logs.Count - 1; i >= 0; i--)
            {
                if (logs[i].Thread.IsAlive)
                    continue;

                bool synced = true;
                foreach (var buffer in logs[i].Buffers)
                {
                    if (!buffer.Synced)
                    {
                        synced = false;
                        break;
                    }
                }

                if (synced)
                    logs.RemoveAt(i);
            }
        }
    }

    /// <summary>Registers the current producer thread and its isolated buffers.</summary>
    private LogThread CreateLogThreads()
    {
        lock (logs)
        {
            var log = new LogThread(Thread.CurrentThread);
            logs.Add(log);
            return log;
        }
    }

    /// <summary>Releases thread-local producer tracking.</summary>
    public void Dispose() => localLogThread.Dispose();
}
