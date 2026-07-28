namespace AlvorKit.Logging;

/// <summary>Writes formatted, caller-annotated entries to an application log stream.</summary>
public partial class Log
{
    /// <summary>Receives formatted entries from producer threads.</summary>
    private readonly LogStream logStream;
    /// <summary>Stores the current minimum severity for lock-free access.</summary>
    private int level = (int)LogLevel.All;

    /// <summary>Creates a producer API over <paramref name="logStream"/>.</summary>
    internal Log(LogStream logStream)
    {
        this.logStream = logStream;
    }

    /// <summary>Gets or sets the least-severe level accepted by this log.</summary>
    public LogLevel Level
    {
        get => (LogLevel)Volatile.Read(ref level);
        set => Volatile.Write(ref level, (int)value);
    }

    /// <summary>Writes an unprefixed message regardless of the configured level.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Raw(string msg,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        var sb = Open();
        sb.Append(msg);
        End(file, line, DateTime.UtcNow, sb, LogLevel.None);
    }

    /// <summary>Commits a completed formatted entry.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void End(string file, int line, DateTime time, LogEntryText sb, LogLevel level)
    {
        sb.Commit(new(time, level, file, line));
    }

    /// <summary>Gets reusable thread-owned UTF-16 storage for a new entry.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private LogEntryText Open() => logStream.OpenText();

    /// <summary>Appends exception details when an exception is present.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendException(ref LogEntryText sb, Exception? exception)
    {
        if (exception != null)
            LogFormat.Exception(ref sb, exception);
    }
}
