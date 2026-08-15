namespace AlvorKit;

/// <summary>Owns the rotating buffers written by one producer thread.</summary>
/// <param name="thread">The producer thread.</param>
internal sealed class LogThread(Thread thread)
{
    /// <summary>Stores the producer's reusable buffers.</summary>
    private LogBuffer[] buffers = [new(), new()];
    /// <summary>Stores reusable formatted text for this producer thread.</summary>
    private readonly TextBuffer text = new();
    /// <summary>Tracks the buffer currently owned by the producer.</summary>
    private int bufferIndex;

    /// <summary>Gets the producer thread.</summary>
    public Thread Thread => thread;
    /// <summary>Gets the buffers currently visible to the collector.</summary>
    public ReadOnlySpan<LogBuffer> Buffers => Volatile.Read(ref buffers);

    /// <summary>Clears and returns the producer's reusable text buffer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LogEntryText OpenText()
    {
        text.Clear();
        return new(this, text);
    }

    /// <summary>Adds one entry to an available producer buffer.</summary>
    public void Add(LogEntry entry, ReadOnlySpan<char> chars)
    {
        var buffer = SelectBuffer(chars.Length);
        buffer.Write(entry, chars);
    }

    /// <summary>Selects a buffer with space for one entry of <paramref name="count"/> characters.</summary>
    private LogBuffer SelectBuffer(int count)
    {
        if (buffers[bufferIndex].Capacity == 0 || buffers[bufferIndex].CharCapacity < count)
            NextBuffer();

        return buffers[bufferIndex];
    }

    /// <summary>Moves to a reusable buffer, expanding or waiting at the configured bound.</summary>
    private void NextBuffer()
    {
        var currentBuffers = Volatile.Read(ref buffers);
        if (currentBuffers.Length < 64)
        {
            int next = NextBufferIndex();
            if (next < 0)
            {
                int previousLength = currentBuffers.Length;
                var expanded = new LogBuffer[previousLength * 2];
                currentBuffers.CopyTo(expanded, 0);
                for (int i = previousLength; i < expanded.Length; i++)
                    expanded[i] = new();

                Volatile.Write(ref buffers, expanded);
                bufferIndex = previousLength;
            }
            else bufferIndex = next;
        }
        else
        {
            int next = NextBufferIndex();

            while (next < 0)
            {
                lock (this)
                {
                    System.Threading.Monitor.Wait(this);
                }

                next = NextBufferIndex();
            }

            bufferIndex = next;
        }

        buffers[bufferIndex].Clear();
    }

    /// <summary>Finds the next fully consumed buffer, or returns <c>-1</c>.</summary>
    private int NextBufferIndex()
    {
        var currentBuffers = Volatile.Read(ref buffers);
        int index = bufferIndex + 1;
        int count = currentBuffers.Length - 1;

        while (count > 0)
        {
            int rindex = index % currentBuffers.Length;
            var buffer = currentBuffers[rindex];

            if (buffer.Synced)
                return rindex;

            index++;
            count--;
        }

        return -1;
    }
}
