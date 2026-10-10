namespace AlvorKit;

/// <summary>Keeps computed results observable without boxing or retaining references in shared state.</summary>
public static class BenchRetain
{
    /// <summary>
    /// Consumes an already computed result after timing. The non-inlined call prevents elimination without
    /// a box that the JIT could allocate inside an earlier measurement interval.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Value<T>(in T value) where T : allows ref struct
    {
    }
}
