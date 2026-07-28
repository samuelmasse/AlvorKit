namespace AlvorKit.Engine.Loop;

/// <summary>Root-owned transient text formatter backed by reusable AlvorKit storage.</summary>
[Root]
public class RootText
{
    /// <summary>Stores text produced during the current root-loop iteration.</summary>
    private readonly TextBuffer text = new();

    /// <summary>Formats one argument into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1>(string format, T1 arg1)
    {
        int start = text.Length;
        CompositeText.Append(text, format, in arg1);
        return text.Span[start..];
    }

    /// <summary>Formats two arguments into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1, T2>(string format, T1 arg1, T2 arg2)
    {
        int start = text.Length;
        CompositeText.Append(text, format, in arg1, in arg2);
        return text.Span[start..];
    }

    /// <summary>Formats three arguments into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
    {
        int start = text.Length;
        CompositeText.Append(text, format, in arg1, in arg2, in arg3);
        return text.Span[start..];
    }

    /// <summary>Formats four arguments into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1, T2, T3, T4>(
        string format,
        T1 arg1,
        T2 arg2,
        T3 arg3,
        T4 arg4)
    {
        int start = text.Length;
        CompositeText.Append(text, format, in arg1, in arg2, in arg3, in arg4);
        return text.Span[start..];
    }

    /// <summary>Formats five arguments into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1, T2, T3, T4, T5>(
        string format,
        T1 arg1,
        T2 arg2,
        T3 arg3,
        T4 arg4,
        T5 arg5)
    {
        int start = text.Length;
        CompositeText.Append(text, format, in arg1, in arg2, in arg3, in arg4, in arg5);
        return text.Span[start..];
    }

    /// <summary>Formats six arguments into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1, T2, T3, T4, T5, T6>(
        string format,
        T1 arg1,
        T2 arg2,
        T3 arg3,
        T4 arg4,
        T5 arg5,
        T6 arg6)
    {
        int start = text.Length;
        CompositeText.Append(text, format, in arg1, in arg2, in arg3, in arg4, in arg5, in arg6);
        return text.Span[start..];
    }

    /// <summary>Formats seven arguments into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1, T2, T3, T4, T5, T6, T7>(
        string format,
        T1 arg1,
        T2 arg2,
        T3 arg3,
        T4 arg4,
        T5 arg5,
        T6 arg6,
        T7 arg7)
    {
        int start = text.Length;
        CompositeText.Append(text, format, in arg1, in arg2, in arg3, in arg4, in arg5, in arg6, in arg7);
        return text.Span[start..];
    }

    /// <summary>Formats eight arguments into the transient root text buffer.</summary>
    public ReadOnlySpan<char> Format<T1, T2, T3, T4, T5, T6, T7, T8>(
        string format,
        T1 arg1,
        T2 arg2,
        T3 arg3,
        T4 arg4,
        T5 arg5,
        T6 arg6,
        T7 arg7,
        T8 arg8)
    {
        int start = text.Length;
        CompositeText.Append(
            text,
            format,
            in arg1,
            in arg2,
            in arg3,
            in arg4,
            in arg5,
            in arg6,
            in arg7,
            in arg8);
        return text.Span[start..];
    }

    /// <summary>Clears transient text while retaining its storage for the next root-loop iteration.</summary>
    internal void Clear() => text.Clear();
}
