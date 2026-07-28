namespace AlvorKit.Text;

/// <summary>Stores up to eight heterogeneous composite-format arguments without boxing.</summary>
/// <typeparam name="T1">The first argument type.</typeparam>
/// <typeparam name="T2">The second argument type.</typeparam>
/// <typeparam name="T3">The third argument type.</typeparam>
/// <typeparam name="T4">The fourth argument type.</typeparam>
/// <typeparam name="T5">The fifth argument type.</typeparam>
/// <typeparam name="T6">The sixth argument type.</typeparam>
/// <typeparam name="T7">The seventh argument type.</typeparam>
/// <typeparam name="T8">The eighth argument type.</typeparam>
/// <param name="arg1">The first argument.</param>
/// <param name="arg2">The second argument.</param>
/// <param name="arg3">The third argument.</param>
/// <param name="arg4">The fourth argument.</param>
/// <param name="arg5">The fifth argument.</param>
/// <param name="arg6">The sixth argument.</param>
/// <param name="arg7">The seventh argument.</param>
/// <param name="arg8">The eighth argument.</param>
/// <param name="count">The number of populated argument slots.</param>
internal readonly struct TextArguments<T1, T2, T3, T4, T5, T6, T7, T8>(
    T1 arg1,
    T2 arg2,
    T3 arg3,
    T4 arg4,
    T5 arg5,
    T6 arg6,
    T7 arg7,
    T8 arg8,
    int count) : ITextArguments
{
    /// <summary>Appends one indexed argument without boxing its value.</summary>
    public void Append(TextBuffer buffer, int index, ReadOnlySpan<char> format)
    {
        if ((uint)index >= (uint)count)
            throw CompositeTextFormatException.Create();

        switch (index)
        {
            case 0: buffer.Append(in arg1, format); break;
            case 1: buffer.Append(in arg2, format); break;
            case 2: buffer.Append(in arg3, format); break;
            case 3: buffer.Append(in arg4, format); break;
            case 4: buffer.Append(in arg5, format); break;
            case 5: buffer.Append(in arg6, format); break;
            case 6: buffer.Append(in arg7, format); break;
            case 7: buffer.Append(in arg8, format); break;
            default: throw CompositeTextFormatException.Create();
        }
    }
}
