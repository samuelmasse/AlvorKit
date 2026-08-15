namespace AlvorKit;

/// <inheritdoc cref="Log"/>
public partial class Log
{
    /// <summary>Writes a debugging entry containing exception details.</summary>
    [OverloadResolutionPriority(1)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug(Exception exception,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipDebug())
            return;
        StartDebug(file, line, out var time, out var sb);
        AppendException(ref sb, exception);
        EndDebug(file, line, time, sb);
    }

    /// <summary>Writes a debugging message and optional exception details.</summary>
    [OverloadResolutionPriority(1)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug(string msg,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipDebug())
            return;
        StartDebug(file, line, out var time, out var sb);
        sb.Append(msg);
        AppendException(ref sb, exception);
        EndDebug(file, line, time, sb);
    }

    /// <summary>Writes one typed value as a debugging entry.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug<T>(in T arg,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipDebug())
            return;
        StartDebug(file, line, out var time, out var sb);
        sb.Append(arg);
        AppendException(ref sb, exception);
        EndDebug(file, line, time, sb);
    }

    /// <summary>Writes a debugging entry from one typed format argument.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug<T>(string format,
        in T arg,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipDebug())
            return;
        StartDebug(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg);
        AppendException(ref sb, exception);
        EndDebug(file, line, time, sb);
    }

    /// <summary>Writes a debugging entry from two typed format arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug<T1, T2>(string format,
        in T1 arg1, in T2 arg2,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipDebug())
            return;
        StartDebug(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg1, in arg2);
        AppendException(ref sb, exception);
        EndDebug(file, line, time, sb);
    }

    /// <summary>Writes a debugging entry from three typed format arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug<T1, T2, T3>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipDebug())
            return;
        StartDebug(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg1, in arg2, in arg3);
        AppendException(ref sb, exception);
        EndDebug(file, line, time, sb);
    }

    /// <summary>Writes a debugging entry from four typed format arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug<T1, T2, T3, T4>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipDebug())
            return;
        StartDebug(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg1, in arg2, in arg3, in arg4);
        AppendException(ref sb, exception);
        EndDebug(file, line, time, sb);
    }

    /// <summary>Writes a debugging entry from five typed format arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug<T1, T2, T3, T4, T5>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4, in T5 arg5,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipDebug())
            return;
        StartDebug(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg1, in arg2, in arg3, in arg4, in arg5);
        AppendException(ref sb, exception);
        EndDebug(file, line, time, sb);
    }

    /// <summary>Writes a debugging entry from six typed format arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug<T1, T2, T3, T4, T5, T6>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4, in T5 arg5, in T6 arg6,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipDebug())
            return;
        StartDebug(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg1, in arg2, in arg3, in arg4, in arg5, in arg6);
        AppendException(ref sb, exception);
        EndDebug(file, line, time, sb);
    }

    /// <summary>Writes a debugging entry from seven typed format arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug<T1, T2, T3, T4, T5, T6, T7>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4, in T5 arg5, in T6 arg6, in T7 arg7,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipDebug())
            return;
        StartDebug(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg1, in arg2, in arg3, in arg4, in arg5, in arg6, in arg7);
        AppendException(ref sb, exception);
        EndDebug(file, line, time, sb);
    }

    /// <summary>Writes a debugging entry from eight typed format arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug<T1, T2, T3, T4, T5, T6, T7, T8>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4, in T5 arg5, in T6 arg6, in T7 arg7, in T8 arg8,
        Exception? exception = null,
        Log? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipDebug())
            return;
        StartDebug(file, line, out var time, out var sb);
        CompositeText.Append(sb.Buffer, format, in arg1, in arg2, in arg3, in arg4, in arg5, in arg6, in arg7, in arg8);
        AppendException(ref sb, exception);
        EndDebug(file, line, time, sb);
    }

    /// <summary>Gets whether debugging entries are disabled by the current threshold.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool SkipDebug() => (LogLevel)Volatile.Read(ref level) < LogLevel.Debug;

    /// <summary>Opens a debugging entry and appends its prefix.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StartDebug(string file, int line, out DateTime time, out LogEntryText sb)
    {
        sb = Open();
        LogFormat.StartDebug(file, line, ref sb, out time);
    }

    /// <summary>Commits a completed debugging entry.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EndDebug(string file, int line, DateTime time, LogEntryText sb) =>
        End(file, line, time, sb, LogLevel.Debug);
}
