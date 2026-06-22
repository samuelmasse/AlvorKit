namespace AlvorKit.Engine.Loop;

/// <summary>Root-owned transient text formatter backed by ZString.</summary>
[Root]
public sealed class RootText
{
    private Utf16ValueStringBuilder sb = ZString.CreateStringBuilder();

    static RootText()
    {
        Fmt((StringBuilder value, Span<char> dst, out int written, ReadOnlySpan<char> fmt) =>
        {
            value.CopyTo(0, dst, value.Length);
            written = value.Length;
            return true;
        });
        Fmt((ReadOnlyMemory<char> value, Span<char> dst, out int written, ReadOnlySpan<char> fmt) =>
        {
            value.Span.CopyTo(dst);
            written = value.Length;
            return true;
        });
        Fmt((Vec2 value, Span<char> dst, out int written, ReadOnlySpan<char> fmt) =>
            Vector((value.X, value.Y, 0, 0), 2, dst, out written, fmt));
        Fmt((Vec2i value, Span<char> dst, out int written, ReadOnlySpan<char> fmt) =>
            Vector((value.X, value.Y, 0, 0), 2, dst, out written, fmt));
        Fmt((Vec2d value, Span<char> dst, out int written, ReadOnlySpan<char> fmt) =>
            Vector((value.X, value.Y, 0, 0), 2, dst, out written, fmt));
        Fmt((Vec3 value, Span<char> dst, out int written, ReadOnlySpan<char> fmt) =>
            Vector((value.X, value.Y, value.Z, 0), 3, dst, out written, fmt));
        Fmt((Vec3i value, Span<char> dst, out int written, ReadOnlySpan<char> fmt) =>
            Vector((value.X, value.Y, value.Z, 0), 3, dst, out written, fmt));
        Fmt((Vec3d value, Span<char> dst, out int written, ReadOnlySpan<char> fmt) =>
            Vector((value.X, value.Y, value.Z, 0), 3, dst, out written, fmt));
        Fmt((Vec4 value, Span<char> dst, out int written, ReadOnlySpan<char> fmt) =>
            Vector((value.X, value.Y, value.Z, value.W), 4, dst, out written, fmt));
        Fmt((Vec4i value, Span<char> dst, out int written, ReadOnlySpan<char> fmt) =>
            Vector((value.X, value.Y, value.Z, value.W), 4, dst, out written, fmt));
        Fmt((Vec4d value, Span<char> dst, out int written, ReadOnlySpan<char> fmt) =>
            Vector((value.X, value.Y, value.Z, value.W), 4, dst, out written, fmt));
    }

    /// <summary>Formats one argument into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1>(string format, T1 arg1) { var start = sb.Length; sb.AppendFormat(format, arg1); return sb.AsSpan()[start..]; }

    /// <summary>Formats two arguments into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1, T2>(string format, T1 arg1, T2 arg2) { var start = sb.Length; sb.AppendFormat(format, arg1, arg2); return sb.AsSpan()[start..]; }

    /// <summary>Formats three arguments into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
    {
        var start = sb.Length;
        sb.AppendFormat(format, arg1, arg2, arg3);
        return sb.AsSpan()[start..];
    }

    /// <summary>Formats four arguments into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        var start = sb.Length;
        sb.AppendFormat(format, arg1, arg2, arg3, arg4);
        return sb.AsSpan()[start..];
    }

    /// <summary>Formats five arguments into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) =>
        FormatCore(format, arg1, arg2, arg3, arg4, arg5);

    /// <summary>Formats six arguments into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) =>
        FormatCore(format, arg1, arg2, arg3, arg4, arg5, arg6);

    /// <summary>Formats seven arguments into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1, T2, T3, T4, T5, T6, T7>(
        string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) =>
        FormatCore(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);

    /// <summary>Formats eight arguments into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1, T2, T3, T4, T5, T6, T7, T8>(
        string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) =>
        FormatCore(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);

    internal void Clear() => sb.Clear();

    private ReadOnlySpan<char> FormatCore<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        var start = sb.Length;
        sb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5);
        return sb.AsSpan()[start..];
    }

    private ReadOnlySpan<char> FormatCore<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        var start = sb.Length;
        sb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6);
        return sb.AsSpan()[start..];
    }

    private ReadOnlySpan<char> FormatCore<T1, T2, T3, T4, T5, T6, T7>(
        string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        var start = sb.Length;
        sb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        return sb.AsSpan()[start..];
    }

    private ReadOnlySpan<char> FormatCore<T1, T2, T3, T4, T5, T6, T7, T8>(
        string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        var start = sb.Length;
        sb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        return sb.AsSpan()[start..];
    }

    private static void Fmt<T>(Utf16ValueStringBuilder.TryFormat<T> format) => Utf16ValueStringBuilder.RegisterTryFormat(format);

    [ExcludeFromCodeCoverage]
    private static bool Vector<T>((T X, T Y, T Z, T W) value, int count, Span<char> dst, out int written, ReadOnlySpan<char> fmt)
        where T : ISpanFormattable
    {
        written = 0;
        dst[written++] = '(';
        if (count > 0 && !Write(value.X, dst, ref written, fmt)) return false;
        if (count > 1 && !WriteAfterComma(value.Y, dst, ref written, fmt)) return false;
        if (count > 2 && !WriteAfterComma(value.Z, dst, ref written, fmt)) return false;
        if (count > 3 && !WriteAfterComma(value.W, dst, ref written, fmt)) return false;
        dst[written++] = ')';
        return true;
    }

    private static bool WriteAfterComma<T>(T value, Span<char> dst, ref int written, ReadOnlySpan<char> fmt)
        where T : ISpanFormattable
    {
        dst[written++] = ',';
        dst[written++] = ' ';
        return Write(value, dst, ref written, fmt);
    }

    [ExcludeFromCodeCoverage]
    private static bool Write<T>(T value, Span<char> dst, ref int written, ReadOnlySpan<char> fmt)
        where T : ISpanFormattable
    {
        if (!value.TryFormat(dst[written..], out var count, fmt, null))
            return false;

        written += count;
        return true;
    }
}
