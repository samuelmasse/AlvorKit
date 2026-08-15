namespace AlvorKit;

/// <summary>Appends strongly typed composite formats without argument arrays or boxing.</summary>
internal static class CompositeText
{
    /// <summary>Appends a composite format containing one argument.</summary>
    public static void Append<T1>(TextBuffer buffer, string format, in T1 arg1) =>
        SingleCompositeTextWriter.Append(buffer, format, in arg1);

    /// <summary>Appends a composite format containing two arguments.</summary>
    public static void Append<T1, T2>(TextBuffer buffer, string format, in T1 arg1, in T2 arg2)
    {
        var args = new TextArguments<
            T1, T2, MissingTextArgument, MissingTextArgument,
            MissingTextArgument, MissingTextArgument, MissingTextArgument, MissingTextArgument>(
            arg1, arg2, default, default, default, default, default, default, 2);
        CompositeTextWriter.AppendMany(buffer, format, in args);
    }

    /// <summary>Appends a composite format containing three arguments.</summary>
    public static void Append<T1, T2, T3>(
        TextBuffer buffer,
        string format,
        in T1 arg1,
        in T2 arg2,
        in T3 arg3)
    {
        var args = new TextArguments<
            T1, T2, T3, MissingTextArgument,
            MissingTextArgument, MissingTextArgument, MissingTextArgument, MissingTextArgument>(
            arg1, arg2, arg3, default, default, default, default, default, 3);
        CompositeTextWriter.AppendMany(buffer, format, in args);
    }

    /// <summary>Appends a composite format containing four arguments.</summary>
    public static void Append<T1, T2, T3, T4>(
        TextBuffer buffer,
        string format,
        in T1 arg1,
        in T2 arg2,
        in T3 arg3,
        in T4 arg4)
    {
        var args = new TextArguments<
            T1, T2, T3, T4,
            MissingTextArgument, MissingTextArgument, MissingTextArgument, MissingTextArgument>(
            arg1, arg2, arg3, arg4, default, default, default, default, 4);
        CompositeTextWriter.AppendMany(buffer, format, in args);
    }

    /// <summary>Appends a composite format containing five arguments.</summary>
    public static void Append<T1, T2, T3, T4, T5>(
        TextBuffer buffer,
        string format,
        in T1 arg1,
        in T2 arg2,
        in T3 arg3,
        in T4 arg4,
        in T5 arg5)
    {
        var args = new TextArguments<
            T1, T2, T3, T4, T5, MissingTextArgument, MissingTextArgument, MissingTextArgument>(
            arg1, arg2, arg3, arg4, arg5, default, default, default, 5);
        CompositeTextWriter.AppendMany(buffer, format, in args);
    }

    /// <summary>Appends a composite format containing six arguments.</summary>
    public static void Append<T1, T2, T3, T4, T5, T6>(
        TextBuffer buffer,
        string format,
        in T1 arg1,
        in T2 arg2,
        in T3 arg3,
        in T4 arg4,
        in T5 arg5,
        in T6 arg6)
    {
        var args = new TextArguments<
            T1, T2, T3, T4, T5, T6, MissingTextArgument, MissingTextArgument>(
            arg1, arg2, arg3, arg4, arg5, arg6, default, default, 6);
        CompositeTextWriter.AppendMany(buffer, format, in args);
    }

    /// <summary>Appends a composite format containing seven arguments.</summary>
    public static void Append<T1, T2, T3, T4, T5, T6, T7>(
        TextBuffer buffer,
        string format,
        in T1 arg1,
        in T2 arg2,
        in T3 arg3,
        in T4 arg4,
        in T5 arg5,
        in T6 arg6,
        in T7 arg7)
    {
        var args = new TextArguments<
            T1, T2, T3, T4, T5, T6, T7, MissingTextArgument>(
            arg1, arg2, arg3, arg4, arg5, arg6, arg7, default, 7);
        CompositeTextWriter.AppendMany(buffer, format, in args);
    }

    /// <summary>Appends a composite format containing eight arguments.</summary>
    public static void Append<T1, T2, T3, T4, T5, T6, T7, T8>(
        TextBuffer buffer,
        string format,
        in T1 arg1,
        in T2 arg2,
        in T3 arg3,
        in T4 arg4,
        in T5 arg5,
        in T6 arg6,
        in T7 arg7,
        in T8 arg8)
    {
        var args = new TextArguments<T1, T2, T3, T4, T5, T6, T7, T8>(
            arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, 8);
        CompositeTextWriter.AppendMany(buffer, format, in args);
    }
}
