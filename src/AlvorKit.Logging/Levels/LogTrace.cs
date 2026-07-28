namespace AlvorKit.Logging;

/// <inheritdoc cref="Log"/>
public partial class Log
{
    /// <summary>Writes a tracing entry containing exception details.</summary>
    [OverloadResolutionPriority(1)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Trace(Exception exception,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipTrace())
            return;
        StartTrace(file, line, out var time, out var sb);
        AppendException(ref sb, exception);
        EndTrace(file, line, time, sb);
    }

    /// <summary>Writes a tracing message and optional exception details.</summary>
    [OverloadResolutionPriority(1)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Trace(string msg,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipTrace())
            return;
        StartTrace(file, line, out var time, out var sb);
        sb.Append(msg);
        AppendException(ref sb, exception);
        EndTrace(file, line, time, sb);
    }

    /// <summary>Writes one typed value as a tracing entry.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Trace<T>(in T arg,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipTrace())
            return;
        StartTrace(file, line, out var time, out var sb);
        sb.Append(arg);
        AppendException(ref sb, exception);
        EndTrace(file, line, time, sb);
    }

    /// <summary>Writes a tracing entry from one typed format argument.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Trace<T>(string format,
        in T arg,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipTrace())
            return;
        StartTrace(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg);
        AppendException(ref sb, exception);
        EndTrace(file, line, time, sb);
    }

    /// <summary>Writes a tracing entry from two typed format arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Trace<T1, T2>(string format,
        in T1 arg1, in T2 arg2,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipTrace())
            return;
        StartTrace(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg1, in arg2);
        AppendException(ref sb, exception);
        EndTrace(file, line, time, sb);
    }

    /// <summary>Writes a tracing entry from three typed format arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Trace<T1, T2, T3>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipTrace())
            return;
        StartTrace(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg1, in arg2, in arg3);
        AppendException(ref sb, exception);
        EndTrace(file, line, time, sb);
    }

    /// <summary>Writes a tracing entry from four typed format arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Trace<T1, T2, T3, T4>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipTrace())
            return;
        StartTrace(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg1, in arg2, in arg3, in arg4);
        AppendException(ref sb, exception);
        EndTrace(file, line, time, sb);
    }

    /// <summary>Writes a tracing entry from five typed format arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Trace<T1, T2, T3, T4, T5>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4, in T5 arg5,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipTrace())
            return;
        StartTrace(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg1, in arg2, in arg3, in arg4, in arg5);
        AppendException(ref sb, exception);
        EndTrace(file, line, time, sb);
    }

    /// <summary>Writes a tracing entry from six typed format arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Trace<T1, T2, T3, T4, T5, T6>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4, in T5 arg5, in T6 arg6,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipTrace())
            return;
        StartTrace(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg1, in arg2, in arg3, in arg4, in arg5, in arg6);
        AppendException(ref sb, exception);
        EndTrace(file, line, time, sb);
    }

    /// <summary>Writes a tracing entry from seven typed format arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Trace<T1, T2, T3, T4, T5, T6, T7>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4, in T5 arg5, in T6 arg6, in T7 arg7,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipTrace())
            return;
        StartTrace(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg1, in arg2, in arg3, in arg4, in arg5, in arg6, in arg7);
        AppendException(ref sb, exception);
        EndTrace(file, line, time, sb);
    }

    /// <summary>Writes a tracing entry from eight typed format arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Trace<T1, T2, T3, T4, T5, T6, T7, T8>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4, in T5 arg5, in T6 arg6, in T7 arg7, in T8 arg8,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipTrace())
            return;
        StartTrace(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg1, in arg2, in arg3, in arg4, in arg5, in arg6, in arg7, in arg8);
        AppendException(ref sb, exception);
        EndTrace(file, line, time, sb);
    }

    /// <summary>Gets whether tracing entries are disabled by the current threshold.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool SkipTrace() => (LogLevel)Volatile.Read(ref level) < LogLevel.Trace;

    /// <summary>Opens a tracing entry and appends its prefix.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StartTrace(string file, int line, out DateTime time, out LogEntryText sb)
    {
        sb = Open();
        LogFormat.StartTrace(file, line, ref sb, out time);
    }

    /// <summary>Commits a completed tracing entry.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EndTrace(string file, int line, DateTime time, LogEntryText sb) =>
        End(file, line, time, sb, LogLevel.Trace);
}
